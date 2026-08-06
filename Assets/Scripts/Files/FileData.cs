using UnityEngine;

[CreateAssetMenu(menuName = "Files/File Data")]
public class FileData : ScriptableObject
{
    public string fileID;
    public string fileName;

    [TextArea(5, 20)]
    public string content;
}