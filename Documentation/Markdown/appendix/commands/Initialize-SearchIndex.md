# Initialize-SearchIndex 
 
Rebuilds the Sitecore index. 
 
## Syntax 
 
Initialize-SearchIndex -Index &lt;ISearchIndex&gt; [-IncludeRemoteIndex] [-AsJob] 
 
Initialize-SearchIndex [-IncludeRemoteIndex] [-Name &lt;String&gt;] [-AsJob] 
 
Initialize-SearchIndex [-Name &lt;String&gt;] [-AsJob] 
 
 
## Detailed Description 
 
The Rebuild-SearchIndex command rebuilds Sitecore index. This command is an alias for Initialize-SearchIndex. 
 
© 2010-2017 Adam Najmanowicz, Michael West. All rights reserved. Sitecore PowerShell Extensions## Aliases
The following abbreviations are aliases for this cmdlet:  
* Rebuild-SearchIndex 
 
## Parameters 
 
### -Index&nbsp; &lt;ISearchIndex&gt; 
 
The index instance. 
 
<table>
    <thead></thead>
    <tbody>
        <tr>
            <td>Aliases</td>
            <td></td>
        </tr>
        <tr>
            <td>Required?</td>
            <td>true</td>
        </tr>
        <tr>
            <td>Position?</td>
            <td>named</td>
        </tr>
        <tr>
            <td>Default Value</td>
            <td></td>
        </tr>
        <tr>
            <td>Accept Pipeline Input?</td>
            <td>true (ByValue, ByPropertyName)</td>
        </tr>
        <tr>
            <td>Accept Wildcard Characters?</td>
            <td>false</td>
        </tr>
    </tbody>
</table> 
 
### -IncludeRemoteIndex&nbsp; &lt;SwitchParameter&gt; 
 
The remote indexing should be triggered. 
 
<table>
    <thead></thead>
    <tbody>
        <tr>
            <td>Aliases</td>
            <td></td>
        </tr>
        <tr>
            <td>Required?</td>
            <td>false</td>
        </tr>
        <tr>
            <td>Position?</td>
            <td>named</td>
        </tr>
        <tr>
            <td>Default Value</td>
            <td></td>
        </tr>
        <tr>
            <td>Accept Pipeline Input?</td>
            <td>false</td>
        </tr>
        <tr>
            <td>Accept Wildcard Characters?</td>
            <td>false</td>
        </tr>
    </tbody>
</table> 
 
### -AsJob&nbsp; &lt;SwitchParameter&gt; 
 
The job created for rebuilding the index should be returned as output. 
 
<table>
    <thead></thead>
    <tbody>
        <tr>
            <td>Aliases</td>
            <td></td>
        </tr>
        <tr>
            <td>Required?</td>
            <td>false</td>
        </tr>
        <tr>
            <td>Position?</td>
            <td>named</td>
        </tr>
        <tr>
            <td>Default Value</td>
            <td></td>
        </tr>
        <tr>
            <td>Accept Pipeline Input?</td>
            <td>false</td>
        </tr>
        <tr>
            <td>Accept Wildcard Characters?</td>
            <td>false</td>
        </tr>
    </tbody>
</table> 
 
### -Name&nbsp; &lt;String&gt; 
 
The name of the index to resume. 
 
<table>
    <thead></thead>
    <tbody>
        <tr>
            <td>Aliases</td>
            <td></td>
        </tr>
        <tr>
            <td>Required?</td>
            <td>false</td>
        </tr>
        <tr>
            <td>Position?</td>
            <td>named</td>
        </tr>
        <tr>
            <td>Default Value</td>
            <td></td>
        </tr>
        <tr>
            <td>Accept Pipeline Input?</td>
            <td>false</td>
        </tr>
        <tr>
            <td>Accept Wildcard Characters?</td>
            <td>false</td>
        </tr>
    </tbody>
</table> 
 
## Inputs 
 
The input type is the type of the objects that you can pipe to the cmdlet. 
 
* None or Sitecore.Jobs.Job 
 
## Outputs 
 
The output type is the type of the objects that the cmdlet emits. 
 
* None 
 
## Notes 
 
Help Author: Adam Najmanowicz, Michael West 
 
## Examples 
 
### EXAMPLE 1 
 
 
 
```powershell   
 
The following rebuilds the index.

PS master:\> Rebuild-SearchIndex -Name sitecore_master_index 
 
``` 
 
### EXAMPLE 2 
 
 
 
```powershell   
 
The following rebuilds the index.

PS master:\> Get-SearchIndex -Name sitecore_master_index | Rebuild-SearchIndex 
 
``` 
 
## Related Topics 
 
* [Resume-SearchIndex](/appendix/commands/Resume-SearchIndex.md)* [Suspend-SearchIndex](/appendix/commands/Suspend-SearchIndex.md)* [Stop-SearchIndex](/appendix/commands/Stop-SearchIndex.md)* [Get-SearchIndex](/appendix/commands/Get-SearchIndex.md)* <a href='https://github.com/SitecorePowerShell/Console/' target='_blank'>https://github.com/SitecorePowerShell/Console/</a><br/>
