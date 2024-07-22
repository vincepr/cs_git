# Basic git implementation written in C#

## Supported commands
- `--help` will list all currently supported commands  
- `csgit init`
- `csgit hash-object`
- `csgit cat-file`

## resources
- https://wyag.thb.lt/
- https://git-scm.com/docs/git-config#_syntax


## working on commit
- get the long hash for the last commit, so we can use the actual obj-file
```
PS C:\Users\vince\RiderProjects\cs_git> git rev-parse HEAD
116441f443b44f1e2fae3f73b072923aa5b8acc2
```