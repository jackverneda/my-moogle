public class DataVector {
    
    public List<string> docpaths = TextReading.getdocs();
    public List<string[]> TokenList= new List<string[]>();
    public Dictionary<string, string> possibletokens= new Dictionary<string, string>();
    public Dictionary<string, double>[] TF= new Dictionary<string,double>[0];
    public Dictionary<string, double> words= new Dictionary<string, double>();
    public int n= 0;
    private void fill(){
        n=docpaths.Count;
        TokenList= new List<string[]>();
        for(int i =0; i<n;i++){
            string s = TextReading.readdoc(docpaths[i]);
            TokenList.Add(s.Split('`', '~', '¡', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-',
                                '_', '=', '+', '[', '{', ']', '}', '|', ';', ':', '"', ',', '<',
                                '.', '>', '/', '¿', '?', ' ', '«', '»', '—', '\n', '\r'));
        }

    }
    public Dictionary<string,double> ex_tf(string[] tokens){
        Dictionary<string, double> dic= new Dictionary<string, double>();
        int maxfreq = 0;
        for(int j=0; j<tokens.Length; j++){
            string w= StringHandling.normalize(tokens[j]);
            string ss= MoogleEngine.Snowball.Stemmer(w);

            if(!possibletokens.ContainsKey(w))
                possibletokens.Add(w,ss);


            if(ss!=""){
                if(dic.ContainsKey(ss)){
                    dic[ss]++;
                } 
                else {
                     dic.Add(ss, 1.0);
                }
                maxfreq = Math.Max(maxfreq ,(int)dic[ss]);
            }
        }

        foreach(KeyValuePair<string, double> pair in dic){
            dic[pair.Key]= (double)(dic[pair.Key]/(maxfreq*(-1.0)));
        } 
        return dic;
    }
    private void filltf(){
        TF = new Dictionary<string, double>[n];
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
                        
                    double log=Math.Log2(n/ni);

                    for(int j=i;j<n;j++)
                        if(TF[j].ContainsKey(pair.Key))
                            TF[j][pair.Key]*=log*(-1.0);
                            
                    words.Add(pair.Key,log);
                    
                }
            }
        }
    }
    
    //Calculates the cos between two vectors
    public static double CompatibleScore(Dictionary<string, double> A, 
                                        Dictionary<string, double> B, 
                                        Dictionary<string, bool> C,
                                        List<string[]> D){
        double sumaw= dot(A,B);
        double sumad= dot(B,B);
        double sumaq= dot(A,B);
     
        foreach(KeyValuePair<string, bool> pair in C){
            sumaq+=4;
            if(B.ContainsKey(pair.Key)){
                sumaw+= 2*B[pair.Key];
                sumad+= Math.Pow(B[pair.Key],2); 
            } 
        }
        
        double result = (sumad!=0 && sumaq!=0)? sumaw/(Math.Sqrt(sumad)*Math.Sqrt(sumaq)): 0;
        return (result>=0)? result : 0 ;    
    }
     public static double dot(Dictionary<string,double> a,Dictionary<string,double> b){
        double ans = 0.0f;
        foreach(var pair in a){  
            if(b.ContainsKey(pair.Key)){
                 ans += (double)(pair.Value * b[pair.Key]);
            }
        }
        return ans;
    }

    
    public DataVector(){
        fill();
        filltf();
        fillidf();
        MoogleEngine.Moogle.loading=false;
    }







}
