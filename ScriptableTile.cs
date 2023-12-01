

//        0
//     ---------
//    /         \
//  5/           \1
//  /             \
//  \             /
//  4\           /2
//    \         /
//     ---------
//        3

using UnityEngine;

[CreateAssetMenu(menuName = "Tile type")]
public class ScriptableTile : ScriptableObject
{
    public Mesh roadMesh;
    public Mesh fencesMesh;
    public Mesh markingsMesh;
    public int[] connections = {0,0,0,0,0,0};
    public Vector3[] propCircles;
    public bool ending;
}