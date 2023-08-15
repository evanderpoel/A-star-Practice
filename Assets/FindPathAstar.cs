using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathMarker
{
    public MapLocation Location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p)
    {
        Location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            PathMarker objMarker = (PathMarker)obj;
            return Location.x == objMarker.Location.x && Location.z == objMarker.Location.z;
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

public class FindPathAstar : MonoBehaviour
{
    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;

    private List<PathMarker> open = new List<PathMarker>();
    private List<PathMarker> closed = new List<PathMarker>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    private PathMarker goalNode;
    private PathMarker startNode;
    private PathMarker lastPos;

    private bool done = false;

    void RemoveAllMarker()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (var marker in markers)
        {
            Destroy(marker);
        }
    }

    void BeginSearch()
    {
        done = false;
        RemoveAllMarker();

        List<MapLocation> locations = new List<MapLocation>();
        for (int i = 1; i < maze.depth-1; i++)
        {
            for (int j = 1; j < maze.width - 1; j++)
            {
                if (maze.map[i, j] != 1)
                {
                    locations.Add(new MapLocation(i, j));
                }
            }
        }
        
        locations.Shuffle();

        Vector3 startLocation = new Vector3(locations[0].x *maze.scale, 0, locations[0].z * maze.scale);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z), 0, 0, 0, 
            Instantiate(start, startLocation, Quaternion.identity), null);
        
        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0, 
            Instantiate(end, goalLocation, Quaternion.identity), null);
        
        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;

    }

    void Search(PathMarker thisNode)
    {
        if (thisNode == null)
        {
            return;
        }
        //goal found
        if (thisNode.Equals(goalNode))
        {
            done = true;
            return;
        }

        foreach (MapLocation dir in maze.directions)
        {
            MapLocation neighbour = dir + thisNode.Location;
            if (maze.map[neighbour.x, neighbour.z] == 1)
            {
                continue;
            }

            if (neighbour.x < 1 || neighbour.x >= maze.depth || neighbour.z < 1 || neighbour.z >= maze.depth)
            {
                continue;
            }

            if (isClosed(neighbour)) continue;

            float G = Vector2.Distance(thisNode.Location.ToVector(), neighbour.ToVector()) + thisNode.G;
            float H = Vector2.Distance(neighbour.ToVector(), goalNode.Location.ToVector());
            float F = G + H;

            GameObject pathBlock = Instantiate(pathP,
                new Vector3(neighbour.x * maze.scale, 0.0f, neighbour.z * maze.scale), Quaternion.identity);

            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            values[0].text = "G: " + G.ToString("0.00");
            values[1].text = "H: " + H.ToString("0.00");
            values[2].text = "F: " + F.ToString("0.00");

            if (!UpdateMarker(neighbour, G, H, F, thisNode))
            {
                open.Add(new PathMarker(neighbour, G, H, F, pathBlock, thisNode));
            }
        }

        open = open.OrderBy(p => p.F).ToList<PathMarker>();
        PathMarker pm = (PathMarker) open.ElementAt(0);
        closed.Add(pm);

        open.RemoveAt(0);
        pm.marker.GetComponent<Renderer>().material = closedMaterial;

        lastPos = pm;

    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach (PathMarker p in open)
        {
            if (p.Location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }

        return false;
    }

    bool isClosed(MapLocation marker)
    {
        foreach (PathMarker p in closed)
        {
            if (p.Location.x == marker.x && p.Location.z == marker.z) return true;
        }

        return false;
    }

    void GetPath()
    {
        RemoveAllMarker();
        PathMarker begin = lastPos;

        while (!startNode.Equals(begin) && begin != null)
        {
            Instantiate(pathP, new Vector3(begin.Location.x * maze.scale, 0, begin.Location.z * maze.scale),
                Quaternion.identity);
            begin = begin.parent;
        }

        Instantiate(pathP, new Vector3(startNode.Location.x * maze.scale, 0, startNode.Location.z * maze.scale),
            Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            BeginSearch();
        }

        if (Input.GetKeyDown(KeyCode.C) && !done)
        {
            Search(lastPos);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            GetPath();
        }
    }
}
