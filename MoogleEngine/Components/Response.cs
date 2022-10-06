public class Response
{
    public Response(  float score, int docindex, string path, int  snippetindex){
        // this.Title = title;
        this.SnippetIndex = snippetindex;
        this.Score = score;
        this.Path= path;
        this.DocIndex= docindex;

        string[] patharray=path.Split('/');
        string title=patharray[patharray.Length-1];
        this.Title=title.Substring(0,title.Length-4);
    }

    public string Title { get; private set; }

    public int SnippetIndex { get; private set; }

    public float Score { get; private set; }

    public string Path {get; private set;}
     
    public int DocIndex {get; private set;}

}