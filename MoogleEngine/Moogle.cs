namespace MoogleEngine;


public static class Moogle
{
    public static bool loading {get; set;} = true;
    public static DataVector A = new DataVector();
    public static SearchResult Query(string query) {
        QueryObj Q=new QueryObj(query);
        KeyValuePair<string,List<KeyValuePair<double, int>>> V = Q.getresponse(query, A);
        int n =  V.Value.Count;
        SearchItem[] items = new SearchItem[n];

        if(V.Value[0].Key==0){
            loading = false;
            return new SearchResult(new SearchItem[0], "Moogle*");
            
        }
        
        for(int i=0; i<n;i++){

            string path=A.docpaths[V.Value[i].Value];
            string doc=TextReading.readdoc(path);
            int size=doc.Length;
            string[] patharray=path.Split('/');
            string title=patharray[patharray.Length-1];
            title=title.Substring(0,title.Length-4);

            items[i]= new SearchItem(title, doc.Substring(0,Math.Min(500,size)),((float)V.Value[i].Key),path);

        }

        loading = false;
        return new SearchResult(items, V.Key);
    }

    
}
