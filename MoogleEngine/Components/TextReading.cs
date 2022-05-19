public class TextReading{

    static string globalpath= "/home/jackson/Escritorio/c#/moogle/Content";

    //This method reads the path of all the docs inside the Content file
    public static List<string> getdocs(){
        List<string> docs= new List<string>();
        foreach(string item in Directory.GetFiles(globalpath, "*.*",SearchOption.AllDirectories)){
            docs.Add(item);
        }
        return docs;  
    }

    //Thismethod reads the whole doc in a path given
    public static string readdoc(string path){
        return File.ReadAllText(path, System.Text.Encoding.UTF8);
    }


}