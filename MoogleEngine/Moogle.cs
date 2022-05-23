namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {
        DataVector A= new DataVector();
        List<KeyValuePair<double, int>> V= DataVector.getresponse(query, A);
        int n= V.Count;
        SearchItem[] items = new SearchItem[n];
        if(V[0].Key==0){
            items= new SearchItem[1];
            items[0]= new SearchItem("", "Lo siento, no se hallaron conincidencias :( ",-1f);
            return new SearchResult(items, "Moogle");
        }
        
        for(int i=0; i<n;i++){
            if(V[i].Key==0 && i==0){

            }
            string path=A.docpaths[V[i].Value];
            string doc=TextReading.readdoc(path);
            int size=doc.Length;
            items[i]= new SearchItem(path.Substring(43), doc.Substring(0,Math.Min(500,size)),((float)V[i].Key));
        }

        return new SearchResult(items, query);
    }
}
