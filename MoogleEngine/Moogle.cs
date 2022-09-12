namespace MoogleEngine;


public static class Moogle
{
    public static DataVector A= new DataVector();

    public static SearchResult Query(string query) {
        KeyValuePair<string,List<KeyValuePair<double, int>>> V= DataVector.getresponse(query, A);
        int n= V.Value.Count;
        SearchItem[] items = new SearchItem[n];

        if(V.Value[0].Key==0){
            // items= new SearchItem[1];
            // items[0]= new SearchItem("", "Lo siento, no se hallaron conincidencias :( ",-1f,"");
            return new SearchResult(new SearchItem[0], "Moogle*");
            
        }
        
        for(int i=0; i<n;i++){
            string path=A.docpaths[V.Value[i].Value];
            string doc=TextReading.readdoc(path);
            int size=doc.Length;
            string[] patharray=path.Split('/');
            items[i]= new SearchItem(patharray[patharray.Length-1]+" "+V.Value[i].Key.ToString(), doc.Substring(0,Math.Min(500,size)),((float)V.Value[i].Key),path);
            // items[i]= new SearchItem(path.Substring(43),V[i].Key.ToString() ,((float)V[i].Key));

                continue;
        }

        return new SearchResult(items, V.Key);
    }

    
}
