namespace MoogleEngine;


public static class Moogle
{
    public static bool loading {get; set;} = true;
    public static DataVector A = new DataVector();
    public static SearchResult Query(string query) {
        QueryObj Q=new QueryObj(query);
        KeyValuePair<string,List<Response>> V = Q.getresponse(query, A);
        int n =  V.Value.Count;
        SearchItem[] items = new SearchItem[n];

        if(V.Value.Count==0){
            loading = false;
            Console.WriteLine("vacio");
            return new SearchResult(new SearchItem[0], "Moogle*");
        }
        
        for(int i=0; i<n;i++){
            string doc=TextReading.readdoc(V.Value[i].Path);
            int size=doc.Length;
            
            items[i]= new SearchItem(
                V.Value[i].Title, 
                doc.Substring(V.Value[i].SnippetIndex,Math.Min(500,size)),
                V.Value[i].Score,
                V.Value[i].Path
            );

        }

        loading = false;
        return new SearchResult(items, V.Key);
    }

    
}
