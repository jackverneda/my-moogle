public class DataVector {
    
    public List<string> docpaths = TextReading.getdocs();
    public List<string[]> TokenList= new List<string[]>();
    public Dictionary<string, string> possibletokens= new Dictionary<string, string>();
    public Dictionary<string, double>[] TF= new Dictionary<string,double>[0];
    public Dictionary<string, List<int>>[] Pos= new Dictionary<string, List<int>>[0];
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
    public Dictionary<string,double> ex_tf(string[] tokens, int i){

        Dictionary<string, double> dic= new Dictionary<string, double>();
        Dictionary<string, List<int>> pos= new Dictionary<string, List<int>>();

        int maxfreq = 0;
        for(int j=0; j<tokens.Length; j++){
            string w= StringHandling.normalize(tokens[j]);
            string ss= MoogleEngine.Snowball.Stemmer(w);

            if(!possibletokens.ContainsKey(w))
                possibletokens.Add(w,ss);


            if(ss!=""){
                if(dic.ContainsKey(ss)){
                    dic[ss]++;
                    pos[ss].Add(j);
                } 
                else {
                     dic.Add(ss, 1.0);
                     pos.Add(ss, new List<int>());
                     pos[ss].Add(j);
                }
                maxfreq = Math.Max(maxfreq ,(int)dic[ss]);
            }
        }

        Pos[i]=pos;
        foreach(KeyValuePair<string, double> pair in dic){
            dic[pair.Key]= (double)(dic[pair.Key]/(maxfreq*(-1.0)));
        } 
        return dic;
    }
    private void filltf(){
        TF = new Dictionary<string, double>[n];
        Pos = new Dictionary<string, List<int>> [n];
        for(int i=0; i<n; i++){
            TF[i]= ex_tf(TokenList[i],i);
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
                                        Dictionary<string, int> C,
                                        List<string[]> D,
                                        Dictionary<string, bool> E,
                                        Dictionary<string,List<int>> F){

        foreach(KeyValuePair<string,bool> pair in E){
            if(pair.Value && !B.ContainsKey(pair.Key))
                return 0;
            else if(!pair.Value && B.ContainsKey(pair.Key))
                return 0;
        }
        
        double sumaw= dot(A,B);
        double sumad= dot(B,B);
        double sumaq= dot(A,B);
     
        foreach(KeyValuePair<string, int> pair in C){
            sumaq+=pair.Value;
            if(B.ContainsKey(pair.Key)){
                sumaw+= pair.Value*B[pair.Key];
                sumad+= Math.Pow(B[pair.Key],2); 
            } 
        }
        for(int i=0;i<D.Count();i++){
            List<List<int>> dist= new List<List<int>>();
            int mx=0;
            for(int j=0;j<D[i].Length;j++){
                if(F.ContainsKey(D[i][j])){
                    dist.Add(F[D[i][j]]);
                    mx=Math.Max(mx,F[D[i][j]].Count());
                }
            }
            double num=DScore(dist);
            sumad+= num/mx;
            
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
    public static double DScore(List<List<int>> V,bool snippet= false){
        double freq=0;
        int n=V.Count();
        int[] pos= new int[n];
        int[] num;
        int max=0;
        int ans=0;
        int norma=5;

        if(snippet)
            norma=20;

        while(Validate(pos,V)){
            num= new int[n];
            int minimun=int.MaxValue;
            int posmin=0;

            for(int i=0;i<n;i++){
                num[i]=V[i][pos[i]];
                if(minimun>=num[i] && pos[i]<V[i].Count()-1){
                    minimun=num[i];
                    posmin=i;
                }             
            }

            double mediaa= media(num);
            int cont=0;
            for(int i=0;i<n;i++){
                if(Math.Abs(num[i]-mediaa)<norma)
                    cont++;
            } 

            if(snippet && cont>max){
                max=cont;
                ans=num[posmin];
                if(max==n)
                    break;
            }
            
            freq+=cont/n;
            pos[posmin]++;
        }
        if(snippet){
            return ans; 
        }
        return freq;
        
    }
    public static bool Validate(int[] pos, List<List<int>> V){
        bool finish=true;
        for(int i=0;i<pos.Length;i++){
            if(pos[i]!=V[i].Count()-1)
                finish=false;
        }
        if(finish)
            return false;
        return true;
    }
    public static double media(int[] array){
        int n=array.Length;
        double suma=0;
        if(n==0)
            return 0;
        for(int i=0;i<n;i++)
            suma+=array[i];
        suma/=(double)n;
        return suma;
    }
    
    public DataVector(){
        fill();
        filltf();
        fillidf();
        MoogleEngine.Moogle.loading=false;
    }







}
