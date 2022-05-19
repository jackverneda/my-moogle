public class StringHandling {
    //Calculate the equality index between two words
    public static int EditDistance( string s1, string s2){
        int[,] M= new int [s1.Length+1, s2.Length+1];
        for(int i=0; i<=s1.Length; i++){
            for(int j=0; j<=s2.Length; j++){
                if(i == 0 && j ==0){
                    M[0,0]= 0;
                    continue;
                }else if(i ==0 || j==0){
                    M[i,j] = 10000;
                    continue;
                }
                if(s1[i-1] == s2[j-1]){
                    M[i,j]= M[i-1,j-1];
                }
                else{
                    M[i,j] = 1 + Math.Min(M[i-1,j],Math.Min(M[i-1,j-1], M[i,j-1]));
                }
            }
        }
        return M[s1.Length, s2.Length];
    }
    //this method make the string normal
    public static string normalize(string s){
            string copys = s.ToLower();
            string result="";
            for(int i=0; i<copys.Length; i++){
                switch (copys[i]){
                    case 'á':
                        result+= "a";
                        break;
                    case 'é':
                        result+= "e";
                        break;
                    case 'í':
                        result+= "i";
                        break;
                    case 'ó':
                        result+="o";
                        break;
                    case 'ú':
                        result+= "u";
                        break;
                    default:
                        if(copys[i]>='a' && copys[i]<='z')
                            result += copys[i];
                        break;
                }
                
            }
            return result;
        }

}