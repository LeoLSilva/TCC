using System.IO;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public static string PathPecasSolos;
    public static string PathDiagramas;
    public static string PathCriacoes;

    private void Awake()
    {
        string basePath = Application.persistentDataPath;

        PathPecasSolos = Path.Combine(basePath, "PecasSolos");
        PathDiagramas = Path.Combine(basePath, "Diagramas");
        PathCriacoes = Path.Combine(basePath, "CriacoesJogadores");

        EnsureDirectoryExists(PathPecasSolos);
        EnsureDirectoryExists(PathDiagramas);
        EnsureDirectoryExists(PathCriacoes);

        Debug.Log("[StorageManager] Estrutura de pastas do Quest verificada e pronta!");
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Debug.Log("Nova pasta criada em: " + path);
        }
    }
}