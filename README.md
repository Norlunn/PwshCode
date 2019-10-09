# PwshCode

Encrypt and/or encode string or objects, to a funny an customizable format. Not very effecient, and output is quite bloated, so this is not for production use.. Idea for this came from a few esoteric languages.. At least, evildoers must have great skills, and too much time on their hand, to be able to crack this. For top security, specify the Secret parameter, and if very bloated, change prefix to none.

[![Pwsh.png](https://i.postimg.cc/zB5617Ty/Pwsh.png)](https://postimg.cc/LqCNtzCS)

# Dependency
This module requires Microsot .NET Core 3.0 or newer installed.

# Installation
```powershell
Install-Module PwshCode
```

# Usage

## Encode
```powershell
"This is a top secret message" | Get-PwshCode | Out-File -FilePath Pwsh.txt
```

## Encode without Prefix
```powershell
"This is a top secret message" | Get-PwshCode -Prefix None | Out-File -FilePath Pwsh.txt
```

## Encode and encrypt
```powershell
"This is a top secret message" | Get-PwshCode -Secret 'Sshh!' | Out-File -FilePath Pwsh.txt
```

## Decode
```powershell
Get-Content -Path Pwsh.txt | Undo-PwshCode
```

## Decode and decrypt
```powershell
Get-Content -Path Pwsh.txt | Undo-PwshCode -Secret 'Sshh!'
```
