
public class QueryObj {
    private string[] wordsq;
    private string query;
    private Dictionary<string,List<string>> stemsyn = new Dictionary<string,List<string> > ();
    // revisar la estructura existance
    private Dictionary<string, bool> Existance =new Dictionary<string, bool> ();
    private Dictionary<string, bool> Rel = new Dictionary<string, bool> ();
    private Dictionary<string, double> NoneOp= new Dictionary<string, double>();
    private List<string[]> closerwords= new List<string[]>();
    private Dictionary<string,string> sustitution = new Dictionary<string, string>();
    private int maxfreqTF=1;
    public QueryObj(string q){
        query=q;
        wordsq = q.Split(' ',',','.');
        oprerators(wordsq);
        wordsq= q.Split(' ',',','.','*','~','!','^');
    }

    private void oprerators(string[] tokens) {
        for(int i=0;i<tokens.Length;i++){
            if(tokens[i][0]=='!' && tokens[i][tokens[i].Length-1]=='*')
                throw new Exception("No pueden usarse operadores de no existencia y de relevancia sobre la misma palabra");
            else if(tokens[i][tokens[i].Length-1]=='*')
                Rel.Add(MoogleEngine.Snowball.Stemmer(StringHandling.normalize(tokens[i].Substring(0,tokens[i].Length-1))), true);
            else if(tokens[i][0]=='!')
                Existance.Add(MoogleEngine.Snowball.Stemmer(StringHandling.normalize(tokens[i].Substring(1))), false);
            else if(tokens[i][0]=='^'){
                string aux=MoogleEngine.Snowball.Stemmer(StringHandling.normalize(tokens[i].Substring(1)));
                Existance.Add(aux, true);
                NoneOp.Add(aux, 1);
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
                        sarray[i]=MoogleEngine.Snowball.Stemmer(StringHandling.normalize(sarray[i]));
                        NoneOp.Add(sarray[i],1);  
                    }
                    closerwords.Add(sarray);                 
                }
                else {
                    string ss=MoogleEngine.Snowball.Stemmer(StringHandling.normalize(tokens[i]) );
                    if(!Rel.ContainsKey(ss) && !Existance.ContainsKey(ss) && !NoneOp.ContainsKey(ss))
                        NoneOp.Add(ss, 1);
                    else if(NoneOp.ContainsKey(ss)){
                        NoneOp[ss]++;
                        maxfreqTF=Math.Max(maxfreqTF,(int)NoneOp[ss]);
                    }
                }
            }
        }

    }
   
    public KeyValuePair<string,List<KeyValuePair<double, int>>> getresponse(string query, DataVector A){
        
        //IDF to the query
        foreach(KeyValuePair<string, double> pair in NoneOp){
            double a = 0.0;
            if(A.words.ContainsKey(pair.Key))
                a= A.words[pair.Key];
            else{
                ValidatorNoneOp(pair.Key,A);
                continue;
            }

            NoneOp[pair.Key]/=maxfreqTF;
            NoneOp[pair.Key] =  (0.4 + (0.6)*NoneOp[pair.Key])*a;     
        }

        List<KeyValuePair<double, int>> response = new  List<KeyValuePair<double, int>>();
        for(int i=0; i < A.n;i++){
            double result = DataVector.CompatibleScore(NoneOp,A.TF[i],Rel,closerwords, Existance);
            response.Add(new KeyValuePair<double, int>(result, i));
        }

        string suggestion="";
        for(int i=0;i<wordsq.Length;i++){
            if(sustitution.ContainsKey(wordsq[i]))
                suggestion += StringHandling.normalize(sustitution[wordsq[i]]) +" ";
            else
                suggestion += StringHandling.normalize(wordsq[i]) +" ";
        }

        response.Sort((x,y) => x.Key.CompareTo(y.Key));
        response.Reverse();
        return new KeyValuePair< string, List<KeyValuePair<double,int>> > (suggestion, response);
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
        
            int nsimword = 1;
            string simword=ss;
            double vsimword= 0;
            string stem= token;

            
            foreach(KeyValuePair<string,string> pair in A.possibletokens){
                int edscore=StringHandling.EditDistance(StringHandling.normalize(pair.Key) , StringHandling.normalize(ss));
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
            }
                    

        }
        
    }

}