public class DataVector {
    
    public List<string> docpaths = TextReading.getdocs();
    public List<string[]> TokenList= new List<string[]>();
    public Dictionary<string, double>[] TF= new Dictionary<string,double>[0];
    
    private void fill(){
        for(int i =0; i< docpaths.Count;i++){
            string s = TextReading.readdoc(docpaths[i]);
            TokenList.Add(s.Split(' ', '.', ','));
        }

    }
    public static void ex_tf(string[] tokens, Dictionary<string, double> dic){
        double maxfreq= 0;
        for(int j=0; j<tokens.Length; j++){
            string ss= StringHandling.normalize(tokens[j]);
            if(dic.ContainsKey(ss)){
                dic[ss]++;
                maxfreq =Math.Max(maxfreq, dic[ss]);
            } else {
                dic.Add(ss, 1);
                maxfreq =Math.Max(maxfreq, dic[ss]);
            }
        }
        foreach(KeyValuePair<string, double> pair in dic){
            dic[pair.Key]/=maxfreq*(-1);
        } 
    }
    private void filltf(){
        TF= new Dictionary<string, double>[docpaths.Count];
        for(int i=0; i<TokenList.Count; i++){
            TF[i]= new Dictionary<string,double>();
            ex_tf(TokenList[i],TF[i]);
        } 
    }
    private void fillidf(){
        for(int i=0;i<TF.Length;i++){
            foreach(KeyValuePair<string,double> pair in TF[i]){
                int ni=1;
                for(int j=i+1;j<TF.Length;j++){
                    if(TF[j].ContainsKey(pair.Key)){
                        ni++;
                    }
                }
                for(int j=i;j<TF.Length;j++){
                    if(TF[j].ContainsKey(pair.Key) && TF[j][pair.Key]<0){
                        TF[j][pair.Key]*=Math.Log2(TF.Length/ni)*(-1);
                    }
                }
            }
        }
    }
    public static double CompatibleScore(Dictionary<string, double> A, Dictionary<string, double> B){
        double suma1=0;
        double suma2=0;
        double suma3=0;
        foreach(KeyValuePair<string,double> pair in A){
            suma3+= pair.Value*pair.Value;
            if(B.ContainsKey(pair.Key) && B[pair.Key]!=0){
                suma1+= pair.Value*B[pair.Key];
                suma2+= B[pair.Key]*B[pair.Key]; 
            }
        }
        // Console.WriteLine($"suma 1: {suma1}, suma 2: {suma2}, suma 3: {suma3}");
         double result= suma1/(Math.Sqrt(suma2)*Math.Sqrt(suma3));
        return result>=0 && result<=1? result: 0 ;    
    }
    public static List<KeyValuePair<double, int>> getresponse(string query, DataVector A){
        List<KeyValuePair<double, int>> response= new List<KeyValuePair<double, int>>();
        string[] tokens = query.Split(' ','.',',');
        Dictionary<string,double> dics= new Dictionary<string, double>();
        DataVector.ex_tf(tokens, dics);
        foreach(KeyValuePair<string, double> pair in dics){
            dics[pair.Key]= 0.4+((-0.6)*pair.Value);
        }
        for(int i=0; i<A.TF.Length;i++){
            double result = DataVector.CompatibleScore(dics,A.TF[i]);
            response.Add(new KeyValuePair<double, int>(result, i));
        }
        response.Sort((x,y) => x.Key.CompareTo(y.Key));
        response.Reverse();
        return response;
    }
    
    public DataVector(){
        fill();
        filltf();
        fillidf();
    }







}
