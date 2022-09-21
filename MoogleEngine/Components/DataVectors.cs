public class DataVector {
    
    public List<string> docpaths = TextReading.getdocs();
    public List<string[]> TokenList= new List<string[]>();
    public List<string> possibletokens= new List<string>();
    public Dictionary<string, double>[] TF= new Dictionary<string,double>[0];
    public Dictionary<string, double> words= new Dictionary<string, double>();
    public int n= 0;
    private void fill(){
        n=docpaths.Count;
        for(int i =0; i<n;i++){
            string s = TextReading.readdoc(docpaths[i]);
            TokenList.Add(s.Split('`', '~', '¡', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-',
                                '_', '=', '+', '[', '{', ']', '}', '|', ';', ':', '"', ',', '<',
                                '.', '>', '/', '¿', '?', ' ', '«', '»', '—', '\n', '\r'));
        }

    }
    public static Dictionary<string,double> ex_tf(string[] tokens,bool queryble= false){
        Dictionary<string, double> dic= new Dictionary<string, double>();
        double maxfreq= 0;
        for(int j=0; j<tokens.Length; j++){
            string ss= StringHandling.normalize(tokens[j], queryble? "*!": "");
            if(ss!=""){
                if(dic.ContainsKey(ss)){
                    dic[ss]++;
                } else if(queryble){
                    if(ss[ss.Length- 1]=='*'){
                        dic.Add(ss = ss.Substring(0,ss.Length-1),100);
                        tokens[j]=ss;
                    }
                    else if(ss[0]=='!'){
                        dic.Add(ss = ss.Substring(1,ss.Length-1),-100);
                        tokens[j]=ss;
                    }
                    else
                        dic.Add(ss, 1.0);
                } else {
                     dic.Add(ss, 1.0);
                }
                maxfreq = Math.Max(maxfreq, dic[ss]);
            }
        }

        foreach(KeyValuePair<string, double> pair in dic){
            dic[pair.Key]= dic[pair.Key]>0 ? dic[pair.Key]/maxfreq*(-1.0) : 0;
            // dic[pair.Key]/=tokens.Length*(-1.0);
        } 
        return dic;
    }
    private void filltf(){
        TF= new Dictionary<string, double>[n];
        for(int i=0; i<n; i++){
            TF[i]= ex_tf(TokenList[i]);
        } 
    }
    private void fillidf(){
        for(int i=0;i<n;i++){
            foreach(KeyValuePair<string,double> pair in TF[i]){
                if(pair.Value<0){
                    int ni=1;

                    for(int j=i+1;j<n;j++)
                        if(TF[j].ContainsKey(pair.Key))
                            ni++;
                        
                    double log=Math.Log(n/ni);

                    for(int j=i;j<n;j++)
                        if(TF[j].ContainsKey(pair.Key))
                            TF[j][pair.Key]*=log*(-1.0);
                            
                    words.Add(pair.Key,log);
                    possibletokens.Add(pair.Key);
                }
            }
        }
    }
    
    //Calculates the cos between two vectors
    public static double CompatibleScore(Dictionary<string, double> A, Dictionary<string, double> B){
        double sumaw=0;
        double sumad=0;
        double sumaq=0;

        foreach(KeyValuePair<string,double> pair in A){
            sumaq+= pair.Value*pair.Value;
            if(B.ContainsKey(pair.Key)){
                sumaw+= pair.Value*B[pair.Key];
                sumad+= B[pair.Key]*B[pair.Key]; 
            } 
        
        }
        double result = (sumad!=0 && sumaq!=0)? sumaw/Math.Sqrt(sumad*sumaq): 0;
        return (result>=0)? result : 0 ;    
    }

    // normalizes all words in a token vector
    private static void tokenValidator(string[] tokens,DataVector A){
        for(int i=0;i<tokens.Length;i++){
            string ss=StringHandling.normalize(tokens[i]);
            // aqui debo validar si el string resultante de normalizar es vacio
            if(!A.words.ContainsKey(ss)){
                int nsimword = 1;
                string simword=ss;
                double vsimword= 0;
                for(int j=0;j<A.possibletokens.Count;j++){
                    int edscore=StringHandling.EditDistance(A.possibletokens[j],ss);
                    double vscore=A.words[A.possibletokens[j]];
                    if(edscore<nsimword){
                        simword=A.possibletokens[j];
                        nsimword=edscore;
                        vsimword=vscore;
                    }
                    else if((edscore==nsimword) && (vscore>vsimword)){
                        simword=A.possibletokens[j];
                        nsimword=edscore;
                        vsimword=vscore;
                    }
                }
                if(simword!=ss){
                  ss=simword;  
                }

            }
            tokens[i]=ss;
        } 
    }
    public static KeyValuePair<string,List<KeyValuePair<double, int>>> getresponse(string query, DataVector A){
        
        string[] tokens = query.Split(' ','.',',');
        tokenValidator(tokens,A);
        Dictionary<string,double> dics = ex_tf(tokens, true);
        
        //IDF to the query
        foreach(KeyValuePair<string, double> pair in dics){
            double a = A.words.ContainsKey(pair.Key)? A.words[pair.Key]: 0.0;
            //aqui iria edit distance
            dics[pair.Key] =  (0.4 + (-0.6)*dics[pair.Key])*a;
            
        }

        List<KeyValuePair<double, int>> response = new  List<KeyValuePair<double, int>>();
        for(int i=0; i < A.n;i++){
            double result = DataVector.CompatibleScore(dics,A.TF[i]);
            response.Add(new KeyValuePair<double, int>(result, i));
        }

        string suggestion="";
        for(int i=0;i<tokens.Length;i++){
            suggestion += tokens[i]+" ";
        }

        response.Sort((x,y) => x.Key.CompareTo(y.Key));
        response.Reverse();
        return new KeyValuePair< string, List<KeyValuePair<double,int>> > (suggestion, response);
    }
    
    public DataVector(){
        fill();
        filltf();
        fillidf();
        MoogleEngine.Moogle.loading=false;
    }







}
