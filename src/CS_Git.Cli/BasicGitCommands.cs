using System.ComponentModel.DataAnnotations;
using Cocona;
using CS_Git.Cli.IO.ConfigFile;

namespace CS_Git.Cli;

public class BasicGitCommands
{
    [Command("add", Description = "Add file contents to the index")]
    public void Add([Argument] string fileOrFolderPath)
    {
        var os = Directory.GetCurrentDirectory();
        Console.WriteLine(os);
        var c1 = Path.Combine(os, ".git");
        
        
        
        Console.WriteLine(fileOrFolderPath); 
        

    }
    
}

class PathExistsAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is string path && (Directory.Exists(path) || File.Exists(path)))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult($"The path '{value}' is not found.");
    }
}

public class Repository
{
     private string _worktree;
     private string _gitdir;
     private string _gitVersion;
     private readonly List<ConfigSection> _conf;

     public Repository(string path, bool force = false)
     {
         this._worktree = path;
         this._gitdir = Path.Combine(path, ".git");
         if (!(force || Path.Exists(_gitdir)))
         {
             throw new Exception($"Not a GIT repository {path}");
         }

         _conf = await ConfigFile.Parse(_gitdir);
         
         // if config missing throw

         if (!force)
         {
             
         }
     }

     private void VerifyDirExists()
     {
         _gitVersion = "12";

     }
}