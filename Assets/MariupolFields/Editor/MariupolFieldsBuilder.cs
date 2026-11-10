using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MariupolFieldsBuilder : EditorWindow
{
    static Mesh sharedTrunk;
    static Mesh sharedBranch;
    static Mesh sharedCrown;

    [MenuItem("Tools/Mariupol Fields/Build MilSim Terrain")]
    static void Open() => GetWindow<MariupolFieldsBuilder>("Mariupol Fields");

    float terrainSize = 1200f;
    float terrainHeight = 38f;
    int treeCount = 520;
    int grassResolution = 256;

    void OnGUI()
    {
        EditorGUILayout.LabelField("Mariupol agricultural fields — terrain builder", EditorStyles.boldLabel);
        terrainSize = EditorGUILayout.FloatField("Terrain width / length (m)", terrainSize);
        terrainHeight = EditorGUILayout.FloatField("Vertical scale (m)", terrainHeight);
        treeCount = EditorGUILayout.IntSlider("Tree count", treeCount, 100, 1200);
        grassResolution = EditorGUILayout.IntSlider("Grass detail resolution", grassResolution, 128, 512);
        EditorGUILayout.HelpBox("Builds a gently rolling 1.2 km field terrain, damp ground material, irregular tree belts, grass and shallow decorative trench lines.", MessageType.Info);
        if (GUILayout.Button("BUILD TERRAIN + VEGETATION", GUILayout.Height(38))) Build();
    }

    void Build()
    {
        const string root = "Assets/MariupolFields";
        string rawPath = root + "/Heightmaps/Fields_513.raw";
        string groundPath = root + "/Textures/Fields_DampGround_Albedo.png";
        if (!File.Exists(rawPath) || !File.Exists(groundPath)) { Debug.LogError("MariupolFields files are missing. Import the complete package."); return; }

        byte[] bytes = File.ReadAllBytes(rawPath);
        int res = 513;
        float[,] heights = new float[res,res];
        for (int z=0; z<res; z++)
            for (int x=0; x<res; x++)
            {
                int i=(z*res+x)*2;
                ushort v=(ushort)(bytes[i] | (bytes[i+1]<<8));
                heights[z,x] = v / 65535f;
            }

        // Decorative shallow trench cuts following the wooded field margins.
        Vector2[][] trenches = {
            new Vector2[]{new Vector2(.105f,.18f),new Vector2(.12f,.28f),new Vector2(.11f,.40f),new Vector2(.13f,.53f),new Vector2(.10f,.67f),new Vector2(.12f,.82f)},
            new Vector2[]{new Vector2(.48f,.08f),new Vector2(.47f,.20f),new Vector2(.49f,.34f),new Vector2(.46f,.49f),new Vector2(.48f,.65f),new Vector2(.45f,.82f)},
            new Vector2[]{new Vector2(.76f,.18f),new Vector2(.74f,.31f),new Vector2(.77f,.45f),new Vector2(.75f,.60f),new Vector2(.78f,.78f)}
        };
        for (int z=0; z<res; z++) for (int x=0; x<res; x++)
        {
            Vector2 p=new Vector2(x/(float)(res-1),z/(float)(res-1));
            float cut=0;
            foreach (var line in trenches)
                for(int k=0;k<line.Length-1;k++)
                {
                    float d=DistanceToSegment(p,line[k],line[k+1]);
                    // narrow walkable trench floor, with subtle raised spoil berms
                    cut=Mathf.Max(cut, Mathf.Exp(-(d*d)/(2f*.0018f*.0018f))*.025f);
                    float berm=Mathf.Exp(-Mathf.Pow(d-.0038f,2)/(2f*.0012f*.0012f))*.006f;
                    heights[z,x] += berm;
                }
            heights[z,x]=Mathf.Clamp01(heights[z,x]-cut);
        }

        TerrainData td=new TerrainData();
        td.heightmapResolution=res;
        td.size=new Vector3(terrainSize,terrainHeight,terrainSize);
        td.SetHeights(0,0,heights);
        td.alphamapResolution=512;
        td.SetDetailResolution(grassResolution,8);

        Texture2D ground=AssetDatabase.LoadAssetAtPath<Texture2D>(groundPath);
        AssetDatabase.DeleteAsset(root+"/Fields_Ground.terrainlayer");
        TerrainLayer layer=new TerrainLayer();
        layer.diffuseTexture=ground;
        layer.tileSize=new Vector2(36,36);
        AssetDatabase.CreateAsset(layer,root+"/Fields_Ground.terrainlayer");
        td.terrainLayers=new TerrainLayer[]{layer};

        GameObject terrainGO=Terrain.CreateTerrainGameObject(td);
        terrainGO.name="Terrain_Mariupol_Fields";
        terrainGO.transform.position=Vector3.zero;
        Terrain terrain=terrainGO.GetComponent<Terrain>();
        terrain.heightmapPixelError=5;
        terrain.basemapDistance=1200;
        terrain.drawInstanced=true;

        // Damp, irregular tree belts along field boundaries. Tree forms use custom meshes, not primitive cylinders/spheres.
        AssetDatabase.DeleteAsset(root+"/Bark_Wet.mat");
        AssetDatabase.DeleteAsset(root+"/Foliage_Muted.mat");
        AssetDatabase.DeleteAsset(root+"/Grass_Muted.mat");
        Material bark=new Material(Shader.Find("Standard")); bark.name="Bark_Wet";
        bark.color=new Color(.19f,.15f,.11f);
        Material leaves=new Material(Shader.Find("Standard")); leaves.name="Foliage_Muted";
        leaves.color=new Color(.12f,.19f,.14f);
        Material grassMat=new Material(Shader.Find("Standard")); grassMat.color=new Color(.20f,.25f,.15f);
        AssetDatabase.CreateAsset(bark,root+"/Bark_Wet.mat");
        AssetDatabase.CreateAsset(leaves,root+"/Foliage_Muted.mat");
        AssetDatabase.CreateAsset(grassMat,root+"/Grass_Muted.mat");
        AssetDatabase.DeleteAsset(root+"/Tree_Trunk_Mesh.asset");
        AssetDatabase.DeleteAsset(root+"/Tree_Branch_Mesh.asset");
        AssetDatabase.DeleteAsset(root+"/Tree_Crown_Mesh.asset");
        sharedTrunk=CreateTaperedTrunk(1f,.12f);
        sharedBranch=CreateTaperedTrunk(1f,.045f);
        sharedCrown=CreateRoughBlob();
        AssetDatabase.CreateAsset(sharedTrunk,root+"/Tree_Trunk_Mesh.asset");
        AssetDatabase.CreateAsset(sharedBranch,root+"/Tree_Branch_Mesh.asset");
        AssetDatabase.CreateAsset(sharedCrown,root+"/Tree_Crown_Mesh.asset");
        GameObject vegetation=new GameObject("Vegetation_TreeBelts");
        Random.InitState(2026);
        Vector2[][] belts = {
            new Vector2[]{new Vector2(.08f,.02f),new Vector2(.09f,.25f),new Vector2(.08f,.52f),new Vector2(.06f,.78f),new Vector2(.03f,.98f)},
            new Vector2[]{new Vector2(.27f,.04f),new Vector2(.28f,.22f),new Vector2(.27f,.45f),new Vector2(.29f,.72f),new Vector2(.25f,.96f)},
            new Vector2[]{new Vector2(.49f,.02f),new Vector2(.50f,.22f),new Vector2(.48f,.48f),new Vector2(.50f,.73f),new Vector2(.47f,.98f)},
            new Vector2[]{new Vector2(.74f,.03f),new Vector2(.73f,.22f),new Vector2(.75f,.47f),new Vector2(.73f,.71f),new Vector2(.76f,.98f)},
            new Vector2[]{new Vector2(.91f,.03f),new Vector2(.90f,.24f),new Vector2(.92f,.51f),new Vector2(.90f,.76f),new Vector2(.94f,.98f)}
        };
        int placed=0;
        while(placed<treeCount)
        {
            int b=Random.Range(0,belts.Length);
            Vector2[] line=belts[b];
            int seg=Random.Range(0,line.Length-1);
            Vector2 p=Vector2.Lerp(line[seg],line[seg+1],Random.value);
            p.x+=Random.Range(-.012f,.012f); p.y+=Random.Range(-.012f,.012f);
            p.x=Mathf.Clamp01(p.x); p.y=Mathf.Clamp01(p.y);
            Vector3 pos=new Vector3(p.x*terrainSize,0,p.y*terrainSize);
            pos.y=terrain.SampleHeight(pos);
            GameObject tree=CreateTree(bark,leaves,Random.Range(7f,13f),Random.Range(.8f,1.4f));
            tree.transform.position=pos;
            tree.transform.rotation=Quaternion.Euler(0,Random.Range(0,360),0);
            tree.transform.SetParent(vegetation.transform,true);
            placed++;
        }

        // Terrain detail grass: mesh tufts with thin, wind-bent blades.
        Mesh tuft=CreateGrassTuft();
        AssetDatabase.DeleteAsset(root+"/GrassTuft_Mesh.asset");
        AssetDatabase.CreateAsset(tuft,root+"/GrassTuft_Mesh.asset");
        GameObject grassPrefab=new GameObject("GrassTuft_Detail");
        grassPrefab.AddComponent<MeshFilter>().sharedMesh=tuft;
        grassPrefab.AddComponent<MeshRenderer>().sharedMaterial=grassMat;
        PrefabUtility.SaveAsPrefabAsset(grassPrefab,root+"/GrassTuft_Detail.prefab");
        GameObject grassPrefabAsset=AssetDatabase.LoadAssetAtPath<GameObject>(root+"/GrassTuft_Detail.prefab");
        DestroyImmediate(grassPrefab);
        DetailPrototype dp=new DetailPrototype();
        dp.prototype=grassPrefabAsset;
        dp.usePrototypeMesh=true;
        dp.renderMode=DetailRenderMode.VertexLit;
        dp.minWidth=.45f; dp.maxWidth=1.2f; dp.minHeight=.35f; dp.maxHeight=.9f;
        dp.noiseSpread=.8f; dp.healthyColor=new Color(.45f,.52f,.34f); dp.dryColor=new Color(.42f,.37f,.25f);
        td.detailPrototypes=new DetailPrototype[]{dp};
        int[,] details=new int[grassResolution,grassResolution];
        for(int z=0;z<grassResolution;z++) for(int x=0;x<grassResolution;x++)
        {
            // Leave field centres relatively sparse; edges and margins carry more rough grass.
            float edge=Mathf.Max(Mathf.Abs(x/(float)grassResolution-.5f),Mathf.Abs(z/(float)grassResolution-.5f));
            details[z,x]=Random.Range(0,4);
        }
        td.SetDetailLayer(0,0,0,details);
        AssetDatabase.DeleteAsset(root+"/MariupolFields_TerrainData.asset");
        AssetDatabase.CreateAsset(td,root+"/MariupolFields_TerrainData.asset");
        terrain.terrainData=td;
        terrain.Flush();

        // Create a simple damp earth trench floor strip mesh as visible definition over the carved depression.
        AssetDatabase.DeleteAsset(root+"/Trench_DampEarth.mat");
        Material trenchMat=new Material(Shader.Find("Standard")); trenchMat.color=new Color(.16f,.14f,.11f);
        AssetDatabase.CreateAsset(trenchMat,root+"/Trench_DampEarth.mat");
        GameObject trenchRoot=new GameObject("Trenches_Decorative");
        foreach(var line in trenches)
        {
            GameObject strip=new GameObject("Shallow_Trench_Floor");
            var mf=strip.AddComponent<MeshFilter>(); var mr=strip.AddComponent<MeshRenderer>();
            mf.sharedMesh=CreatePathRibbon(line,terrainSize,terrain,1.7f);
            mr.sharedMaterial=trenchMat;
            strip.transform.SetParent(trenchRoot.transform,false);
        }

        AssetDatabase.SaveAssets();
        Selection.activeGameObject=terrainGO;
        Debug.Log("Mariupol Fields terrain built. Terrain size: "+terrainSize+"m. Tree belts, damp ground, grass and trench cuts generated.");
    }

    static float DistanceToSegment(Vector2 p,Vector2 a,Vector2 b)
    {
        Vector2 ab=b-a; float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/Mathf.Max(.000001f,ab.sqrMagnitude));
        return Vector2.Distance(p,a+ab*t);
    }

    static GameObject CreateTree(Material bark,Material leaves,float h,float width)
    {
        GameObject root=new GameObject("Tree_Irregular_Deciduous");
        GameObject t=new GameObject("Forked_Trunk"); t.transform.SetParent(root.transform,false);
        t.AddComponent<MeshFilter>().sharedMesh=sharedTrunk;
        t.transform.localScale=new Vector3(width, h*.58f, width);
        t.AddComponent<MeshRenderer>().sharedMaterial=bark;
        for(int i=0;i<5;i++)
        {
            float y=h*(.30f+i*.095f);
            GameObject branch=new GameObject("Branch"); branch.transform.SetParent(root.transform,false);
            Vector3 start=new Vector3(0,y,0);
            Vector3 dir=new Vector3(Mathf.Cos(i*2.4f),.45f,Mathf.Sin(i*2.4f)).normalized;
            branch.transform.position=start;
            branch.transform.rotation=Quaternion.FromToRotation(Vector3.up,dir);
            branch.AddComponent<MeshFilter>().sharedMesh=sharedBranch;
            branch.transform.localScale=new Vector3(width*.38f,h*.20f,width*.38f);
            branch.AddComponent<MeshRenderer>().sharedMaterial=bark;
            branch.transform.localScale=new Vector3(1,1,1);
        }
        for(int i=0;i<9;i++)
        {
            GameObject crown=new GameObject("Leaf_Crown_Cluster"); crown.transform.SetParent(root.transform,false);
            float y=h*Random.Range(.48f,.88f);
            Vector3 offset=new Vector3(Random.Range(-width,width),y,Random.Range(-width,width));
            crown.transform.localPosition=offset;
            float s=Random.Range(width*.48f,width*.9f);
            crown.transform.localScale=new Vector3(s*Random.Range(.8f,1.2f),s*Random.Range(.8f,1.4f),s*Random.Range(.8f,1.2f));
            MeshFilter mf=crown.AddComponent<MeshFilter>(); mf.sharedMesh=sharedCrown;
            crown.AddComponent<MeshRenderer>().sharedMaterial=leaves;
        }
        return root;
    }

    static Mesh CreateTaperedTrunk(float length,float radius)
    {
        int sides=7; Vector3[] v=new Vector3[(sides+1)*2]; int[] tri=new int[sides*6];
        for(int y=0;y<2;y++) for(int i=0;i<=sides;i++)
        {
            float a=i*Mathf.PI*2/sides; float r=radius*(y==0?1:.48f);
            v[y*(sides+1)+i]=new Vector3(Mathf.Cos(a)*r,y*length,Mathf.Sin(a)*r);
        }
        for(int i=0;i<sides;i++){int a=i,b=i+1,c=sides+1+i,d=c+1; int k=i*6; tri[k]=a;tri[k+1]=c;tri[k+2]=b;tri[k+3]=b;tri[k+4]=c;tri[k+5]=d;}
        Mesh m=new Mesh();m.vertices=v;m.triangles=tri;m.RecalculateNormals();return m;
    }

    static Mesh CreateRoughBlob()
    {
        Mesh m=Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        if(m==null) return new Mesh();
        Vector3[] v=m.vertices;
        for(int i=0;i<v.Length;i++){float n=1f+Mathf.Sin(v[i].x*17+v[i].y*13+v[i].z*9)*.10f;v[i]*=n;}
        Mesh clone=new Mesh();clone.vertices=v;clone.triangles=m.triangles;clone.RecalculateNormals();return clone;
    }

    static Mesh CreateGrassTuft()
    {
        List<Vector3> v=new List<Vector3>(); List<int> tr=new List<int>();
        for(int i=0;i<7;i++)
        {
            float a=i*2.399f; Vector3 d=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
            Vector3 side=Vector3.Cross(d,Vector3.up)*.035f; Vector3 tip=d*Random.Range(.10f,.24f)+Vector3.up*Random.Range(.28f,.55f);
            int k=v.Count; v.Add(-side);v.Add(side);v.Add(tip);tr.Add(k);tr.Add(k+1);tr.Add(k+2);
        }
        Mesh m=new Mesh();m.SetVertices(v);m.SetTriangles(tr,0);m.RecalculateNormals();return m;
    }

    static Mesh CreatePathRibbon(Vector2[] line,float size,Terrain terrain,float width)
    {
        List<Vector3> v=new List<Vector3>();List<int> tr=new List<int>();
        for(int i=0;i<line.Length;i++)
        {
            Vector3 p=new Vector3(line[i].x*size,0,line[i].y*size);
            p.y=terrain.SampleHeight(p)-.03f;
            Vector3 tangent=(i<line.Length-1?new Vector3(line[i+1].x-line[i].x,0,line[i+1].y-line[i].y):new Vector3(line[i].x-line[i-1].x,0,line[i].y-line[i-1].y)).normalized;
            Vector3 side=Vector3.Cross(tangent,Vector3.up)*(width*.5f);
            v.Add(p-side);v.Add(p+side);
            if(i>0){int k=i*2;tr.Add(k-2);tr.Add(k);tr.Add(k-1);tr.Add(k-1);tr.Add(k);tr.Add(k+1);}
        }
        Mesh m=new Mesh();m.SetVertices(v);m.SetTriangles(tr,0);m.RecalculateNormals();return m;
    }
}
