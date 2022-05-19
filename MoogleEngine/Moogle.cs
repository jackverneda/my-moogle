namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {
        DataVector A= new DataVector();
        List<KeyValuePair<double, int>> V= DataVector.getresponse(query, A);
        int n= Math.Min(V.Count,3);
        SearchItem[] items = new SearchItem[3];
        
        for(int i=0; i<n;i++){
            items[i]= new SearchItem(A.docpaths[V[i].Value], TextReading.readdoc(A.docpaths[V[i].Value]),((float)V[i].Key));
        }

        return new SearchResult(items, query);
    }
}
