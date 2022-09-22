public class TextReading{

    static string globalpath= "..//Content//";

    //This method read the path of all the docs inside the Content file
    public static List<string> getdocs(){
        List<string> docs= new List<string>();
        foreach(string item in Directory.GetFiles(globalpath, "*.*",SearchOption.AllDirectories)){
            docs.Add(item);
        }
        return docs;  
    }

    //Thismethod read the whole doc in a path given
    public static string readdoc(string path){
        return File.ReadAllText(path, System.Text.Encoding.UTF8);
    }

    // This method read a whole document by lines
    public static List<string> readlines(string path){
        string[] doc = File.ReadAllLines(path);
        List<string> words = new List<string>();
        for (int i = 0; i < doc.Length; i++)
            words.Add(doc[i]);
        return words;
    }

}