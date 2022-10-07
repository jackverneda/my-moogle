

public class QueryObj {
    private string[] wordsq;
    private string query;
    private Dictionary<string,List<string>> stemsyn = new Dictionary<string,List<string> > ();
    // revisar la estructura existance
    private Dictionary<string, bool> Existance =new Dictionary<string, bool> ();
    private Dictionary<string, int> Rel = new Dictionary<string, int> ();
    private Dictionary<string, double> NoneOp= new Dictionary<string, double>();
    private List<string[]> closerwords= new List<string[]>();
    private Dictionary<string,string> sustitution = new Dictionary<string, string>();
    private int maxfreqTF=1;
    public List<string> tosnippet= new List<string>();
    public int index_snippet=0;
    public QueryObj(string q){
        query=q;
        wordsq = q.Split(' ',',','.');
        oprerators(wordsq);
        wordsq= q.Split(' ',',','.','*','~','!','^');
    }

    private void oprerators(string[] tokens) {
        for(int i=0;i<tokens.Length;i++){
            if(tokens[i]=="")
                continue;
            if(tokens[i][0]=='!' && tokens[i][tokens[i].Length-1]=='*')
                throw new Exception("No pueden usarse operadores de no existencia y de relevancia sobre la misma palabra");
            else if(tokens[i][tokens[i].Length-1]=='*'){
                int cont=1;
                for(int j=tokens[i].Length-1;j>=0;j--){
                    if(tokens[i][j]!='*')
                        break;
                    cont++;
                }

                string aux=MoogleEngine.Snowball.Stemmer(StringHandling.normalize(tokens[i].Substring(0,tokens[i].Length)));
                if(aux=="")
                    continue;
                Rel.Add(aux, cont);
                tosnippet.Add(aux);
                Console.WriteLine("bien");
            }
            else if(tokens[i][0]=='!')
                Existance.Add(MoogleEngine.Snowball.Stemmer(StringHandling.normalize(tokens[i].Substring(1))), false);
            else if(tokens[i][0]=='^'){
                string aux=MoogleEngine.Snowball.Stemmer(StringHandling.normalize(tokens[i].Substring(1)));
                Existance.Add(aux, true);
                NoneOp.Add(aux, 1);
                tosnippet.Add(aux);
            }
            else{
                bool closeOp=false;
                for(int j=0;j<tokens[i].Length;j++){
                    if(tokens[i][j]=='~'){
                        closeOp = true;
                        break;
                    }
                }
                if(closeOp){
                    string[] sarray=tokens[i].Split('~');
                    for(int j=0;j<sarray.Length;j++){
                        sarray[j]=MoogleEngine.Snowball.Stemmer(StringHandling.normalize(sarray[j]));
                        NoneOp.Add(sarray[j],1);
                        tosnippet.Add(sarray[j]);
                    }
                    closerwords.Add(sarray);                 
                }
                else {
                    string ss=MoogleEngine.Snowball.Stemmer(StringHandling.normalize(tokens[i]) );
                    if(!Rel.ContainsKey(ss) && !Existance.ContainsKey(ss) && !NoneOp.ContainsKey(ss)){
                        NoneOp.Add(ss, 1);
                        tosnippet.Add(ss);
                    }
                    else if(NoneOp.ContainsKey(ss)){
                        NoneOp[ss]++;
                        maxfreqTF=Math.Max(maxfreqTF,(int)NoneOp[ss]);
                    }
                }
            }
        }

    }
   
    public KeyValuePair<string,List<Response>> getresponse(string query, DataVector A){
        
        //IDF to the query
        List<string> Wrong= new List<string>();

        foreach(KeyValuePair<string, double> pair in NoneOp){
            double a = 0.0;
            if(A.words.ContainsKey(pair.Key))
                a= A.words[pair.Key];
            else{
                Wrong.Add(pair.Key);
                continue;
            }

            NoneOp[pair.Key]/=maxfreqTF;
            NoneOp[pair.Key] =  (0.4 + (0.6)*NoneOp[pair.Key])*a;     
        }

        for(int i=0;i<Wrong.Count;i++)
            ValidatorNoneOp(Wrong[i],A);

        List<Response> response = new  List<Response>();
        for(int i=0; i < A.n;i++){
            double result = DataVector.CompatibleScore(NoneOp,A.TF[i],Rel,closerwords, Existance, A.Pos[i]);

            if(result<0.21)
                continue;

            Console.WriteLine(A.docpaths[i]);

            List<List<int>> dist= new List<List<int>>();
            for(int j=0;j<tosnippet.Count();j++){
                if(!NoneOp.ContainsKey(tosnippet[j]) && !Rel.ContainsKey(tosnippet[j]))
                    continue;
                if(NoneOp.ContainsKey(tosnippet[j]) && NoneOp[tosnippet[j]]<0.2)
                    continue;

                if(A.Pos[i].ContainsKey(tosnippet[j]))
                    dist.Add(A.Pos[i][tosnippet[j]]);
            }
                
            int snippet=(int)DataVector.DScore(dist, true);
            int pos=snippet-1;

            for(int k=0;k<snippet;k++)
                pos+=A.TokenList[i][k].Length;

            Console.WriteLine(pos);

            response.Add(new Response((float)result, i, A.docpaths[i], pos>=0? pos: 0));
        }
    
        string suggestion="";
        for(int i=0;i<wordsq.Length;i++){
            if(sustitution.ContainsKey(wordsq[i]))
                suggestion += StringHandling.normalize(sustitution[wordsq[i]]) +" ";
            else
                suggestion += StringHandling.normalize(wordsq[i]) +" ";
        }


        response.Sort((x,y) => x.Score.CompareTo(y.Score));
        response.Reverse();
        return new KeyValuePair< string, List<Response> > (suggestion, response);
    }

    private void ValidatorNoneOp(string token, DataVector A){
        string ss=token;
            
        int p=0;
        if(!A.words.ContainsKey(token)){
            for(int i=0;i<wordsq.Length;i++){
                if(wordsq[i].Contains(token)){
                    ss=StringHandling.normalize(wordsq[i]);
                    p=i;
                    break;
                }
            }
        
            int nsimword = ss.Length/2;
            string simword=ss;
            double vsimword= 0;
            string stem= token;

            
            foreach(KeyValuePair<string,string> pair in A.possibletokens){
                int edscore=StringHandling.EditDistance(StringHandling.normalize(pair.Key) , ss);
                if(!A.words.ContainsKey(pair.Value))
                    continue;
                double vscore=A.words[pair.Value];

                if(edscore<nsimword){
                    simword=pair.Key;
                    nsimword=edscore;
                    vsimword=vscore;
                    stem=pair.Value;
                }

                else if((edscore==nsimword) && (vscore>vsimword)){
                    simword=pair.Key;
                    nsimword=edscore;
                    vsimword=vscore;
                    stem=pair.Value;
                }
            
            }
            if(simword!=ss){
                NoneOp.Remove(token);
                NoneOp.Add(stem,vsimword);
                sustitution.Add(ss,simword);
                tosnippet.Add(stem);
            }
                    

        }
        
    }

}
