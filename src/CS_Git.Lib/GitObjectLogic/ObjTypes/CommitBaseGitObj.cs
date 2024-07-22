using System.Security.Principal;
using System.Text;
using CS_Git.Lib.GitObjectLogic.Models;
using CS_Git.Lib.RepositoryLogic;

namespace CS_Git.Lib.GitObjectLogic.ObjTypes;

// $ git cat-file commit 116441f443b44f1e2fae3f73b072923aa5b8acc2

// tree df5626d6dbaaaf581ff11f0efddcea00f38dae99
// parent 4b76882c4a0e7a69764f7042b0785ef6e835b7a9                      < 0-xx parents (commit before, merges for > 1)
// author vince <vincepr@gmx.de> 1720965331 +0200
// committer vince <vincepr@gmx.de> 1720965331 +0200    
//                                                                      < newline denotes that commit message follows
// Initial commit blah
// blah blah blah
public record CommitBaseGitObj : BaseGitObj
{
    /// <summary> Identifies the top-level tree describing the repo at this snapshot. </summary>
    public required GitSha1 Tree { get; set; }
    /// <summary> Optional List of parent commit objects. Eg. 2 parents happen in merge commits. More than 2 are possible with 'git merge -Xoctopus'.</summary>
    public required List<GitSha1>? Parents { get; set; }
    public required GitPersonInfo Author { get; set; }
    public required GitPersonInfo Committer { get; set; }
    public required string Message { get; set; }
    public required string? PgpSignature { get; set; }
    
    public CommitBaseGitObj(byte[] Content) : base(Content)
    {
    }

    internal const string TypeName = "commit";

    public override Task<GitSha1> Serialize(Repository repo)
    {
        throw new NotImplementedException();
    }

    public string SerializeToUtf8()
    {
        var builder = new StringBuilder();
        builder.Append($"tree {Tree.HashAsString}\n");
        foreach (var p in Parents ?? [])
            builder.Append($"parent {p.HashAsString}\n");
        builder.Append($"author {Author.ToString()}\n");
        builder.Append($"committer {Committer.ToString()}\n");
        
        if (PgpSignature is not null)
            foreach (var line in PgpSignature.Split('\n'))
                builder.Append($" {line}\n");
        
        builder.Append('\n');
        builder.Append(Message);
        return builder.ToString();
    }
    
    public static async Task<CommitBaseGitObj> Deserialize(byte[] content)
    {
        // TODO: actually multiline is allowed, every following line will use on ' ' to indicate it is still same-key!
        var raw = Encoding.UTF8.GetString(content);
        var reader = new StringReader(raw);
        
        var treeSha = await ExpectIdentifierSha(reader, expectedString: "tree");

        List<GitSha1> parents = [];
        while (reader.Peek() != -1 && reader.Peek() == 112)  // 112 == 'p' 
            parents.Add(await ExpectIdentifierSha(reader, expectedString: "parent"));
        
        var author = await ExpectIdentifierPersonInfo(reader, expectedString: "author");
        
        var committer = await ExpectIdentifierPersonInfo(reader, expectedString: "committer");
        
        string? signature = null;
        // handle pgp signature:
        if (reader.Peek() == 112) // 112 == 'p'
        {
            var firstLine = await reader.ReadLineAsync();
            var gpgsig = firstLine!.Substring(0, 8);
            if (gpgsig != "gpgsig")
                throw new NotImplementedException("Currently only supporting pgpsig to sign commit.");

            while (reader.Peek() != 32) // 10 == ' ' because every subsequent multiline will be lead with a ' '
            {
                var unescapedLine = (await reader.ReadLineAsync())![1..];
                signature += unescapedLine + "\n";
            }
        }
        

        if (reader.Read() != 10)
            throw new DataMisalignedException($"Expected {nameof(CommitBaseGitObj)} to have a newline before message");
        var message = await reader.ReadToEndAsync();

        return new CommitBaseGitObj(content)
        {
            Tree = treeSha,
            Parents = parents.Count > 0
                ? parents
                : null,
            Author = author,
            Committer = committer,
            Message = message,
            PgpSignature = signature,
        };
    }

    private static async Task<GitSha1> ExpectIdentifierSha(StringReader reader, string expectedString)
    {
        var line = await reader.ReadLineAsync() 
            ?? throw new DataMisalignedException($"Expected {nameof(CommitBaseGitObj)} to be '{expectedString} sha1' but was null'");
        var words = line.Split(' ');
        if (words.Length != 2 || words.First() != expectedString)
            throw new DataMisalignedException($"Parsing {nameof(CommitBaseGitObj)}. Bad length of row: {expectedString}");
        return GitSha1.FromHexString(words[1]);
    }
    
    private static async Task<GitPersonInfo> ExpectIdentifierPersonInfo(StringReader reader, string expectedString)
    {
        var line = await reader.ReadLineAsync() 
            ?? throw new DataMisalignedException($"Expected {nameof(CommitBaseGitObj)} to be '{expectedString} name mail timezone' but was null'");
        var words = line.Split(' ').ToList();
        if (words.Count < 4 || words.First() != expectedString)
            throw new DataMisalignedException($"Parsing {nameof(CommitBaseGitObj)}. Bad length of row: {expectedString}");
        var personType = (words[0]) switch
        {
            "author" => GitPersonInfo.PersonType.author,
            "committer" => GitPersonInfo.PersonType.committer,
            _ => throw new DataMisalignedException($"Parsing {nameof(CommitBaseGitObj)}, row: {expectedString}, unknown type: {words.First()}"),
        };
        words.RemoveAt(0);
        var timezone = words.Last();
        words.RemoveAt(words.Count - 1);
        
        var seconds = uint.Parse(words.Last());
        words.RemoveAt(words.Count - 1);
        
        var mail = words.Last().TrimStart('<').TrimEnd('>');
        words.RemoveAt(words.Count - 1);

        if (words.Count < 1)
            throw new DataMisalignedException($"Parsing {nameof(GitPersonInfo)}, type: {expectedString}, no Name found");
        
        return new GitPersonInfo
        {
            Type = personType,
            Mail = mail,
            Seconds = seconds,
            TimeZone = timezone,
            Name = string.Join(' ', words),
        };
    }

    public override GitSha1 Hash() => HashBaseImplementation(TypeName, Content);

    public override string ToString() => Encoding.UTF8.GetString(Content);
}