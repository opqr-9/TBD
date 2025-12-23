using System;
using System.Collections.Generic;
using UnityEngine;

public enum Delete
{
    OUT,
    IN,
    NO
}

//网格数据
public class DigitalMesh
{
    public List<int> pointsRelativeOrder;
    public Dictionary<int,Vector2> points;
    public HashSet<Triangle> triangles;
    public HashSet<Line> lines;
    public Dictionary<Line,List<Triangle>> lineTriangleDictionary;          //线-三角字典
    public Dictionary<int,List<Triangle>> pointIndexTriangleDictionary;     //点(索引)-三角字典

    private int counter = 0;

    public DigitalMesh(List<int> pointsRelativeOrder = null, Dictionary<int, Vector2> points = null,
        HashSet<Triangle> triangles = null, HashSet<Line> lines = null,
        Dictionary<Line, List<Triangle>> lineTriangleDictionary = null,
        Dictionary<int, List<Triangle>> pointIndexTriangleDictionary = null)
    {
        this.pointsRelativeOrder=pointsRelativeOrder??new List<int>();
        this.points = points ?? new Dictionary<int,Vector2>();
        this.triangles = triangles ?? new HashSet<Triangle>();
        this.lines = lines?? new HashSet<Line>();
        this.lineTriangleDictionary = lineTriangleDictionary ?? new Dictionary<Line,List<Triangle>>();
        this.pointIndexTriangleDictionary = pointIndexTriangleDictionary ?? new Dictionary<int, List<Triangle>>();
    }
    
    //根据point将newPointsIndices按顺时针排序（冒泡）
    void Clockwise(List<int> newPointsIndices,Vector2 point)
    {
        bool flag=true;
        Vector2 basevector = points[newPointsIndices[0]]-point;
        while (flag)
        {
            flag = false;
            for (int i = 1; i < newPointsIndices.Count-1; i++)
            {
                Vector2 curVector = points[newPointsIndices[i]]-point;
                Vector2 nextVector = points[newPointsIndices[i+1]]-point;
                Vector3 curCross = -Vector3.Cross(curVector, basevector);
                Vector3 nextCross = -Vector3.Cross(nextVector, basevector);
                float curAngle = Vector2.Angle(basevector, curVector);
                float nextAngle = Vector2.Angle(basevector, nextVector);
                if (curCross.z < 0 && nextCross.z >= 0)
                {
                    (newPointsIndices[i], newPointsIndices[i + 1]) = (newPointsIndices[i + 1], newPointsIndices[i]);
                    flag = true;
                }
                else if (curCross.z >= 0 && nextCross.z >= 0 && curAngle > nextAngle)
                {
                    (newPointsIndices[i], newPointsIndices[i + 1]) = (newPointsIndices[i + 1], newPointsIndices[i]);
                    flag = true;
                }
                else if (curCross.z < 0 && nextCross.z < 0 && curAngle < nextAngle)
                {
                    (newPointsIndices[i], newPointsIndices[i + 1]) = (newPointsIndices[i + 1], newPointsIndices[i]);
                    flag = true;
                }
            }
        }
    }

    //批量添加
    void Triangulation(List<Vector2> addPoints)
    {
        List<int> indices = AddNewPointsToIndices(addPoints);
        for (int i = 0; i < indices.Count; i++)
        {
            Triangulation(indices[i]);
        }
    }
    
    //自动生成约束边,将相邻的点生成约束边
    List<Line> ConstrainedLines(List<int> pointsIndices)
    {
        List<Line> constrainedLines = new List<Line>();
        for (int i = 0; i < pointsIndices.Count; i++)
        {
            Line l = new Line(pointsIndices[i],pointsIndices[(i + 1)%pointsIndices.Count],this);
            if (!lines.Contains(l))
            {
                constrainedLines.Add(l);
            }
        }
        return constrainedLines;
    }
    
    //添加点并且返回其哈希索引
    private List<int> AddNewPointsToIndices(List<Vector2> addPoints)
    {
        List<int> Indices = new List<int>();
        for (int i = 0; i < addPoints.Count; i++)
        {
            while (points.ContainsKey(counter))
            {
                counter++;
                counter %= Int32.MaxValue;
            }

            if (points.ContainsValue(addPoints[i]))
            {
                continue;
            }
            // int pointhc=addPoints[i].GetHashCode();
            points.Add(counter,addPoints[i]);
            pointsRelativeOrder.Add(counter);
            Indices.Add(counter);
        }
        return Indices;
    }

    public void Init(List<Vector2> pointsBuffer)
    {
        Vector2 leftdown=pointsBuffer[0], rightup=pointsBuffer[0];
        for (int i = 0; i < pointsBuffer.Count; i++)
        {
            leftdown = Vector2.Min(pointsBuffer[i], leftdown);
            rightup = Vector2.Max(pointsBuffer[i], rightup);
        }
        leftdown-=Vector2.one;
        rightup+=Vector2.one;
        Vector2 rightdown = new Vector2(rightup.x, leftdown.y);
        Vector2 leftup = new Vector2(leftdown.x, rightup.y);
        List<Vector2> pointBuffer = new List<Vector2>
        {
            leftdown,
            rightdown,
            rightup,
            leftup
        };
        List<int> indices=AddNewPointsToIndices(pointBuffer);
        AddToMesh(new Triangle(indices[0],indices[1],indices[2],this));
        AddToMesh(new Triangle(indices[1],indices[2],indices[3],this));
        ConstrainedTriangulation(pointsBuffer,Delete.OUT);


        for (int i = 0; i < indices.Count; i++)
        {
            points.Remove(indices[i]);
        }
        
        pointsBuffer.Clear();
        // for (int i = 0; i < 4; i++)
        // {
        //     for (int j = 0; j < pointIndexTriangleDictionary[i].Count; j++)
        //     {
        //         RemoveFromMesh(pointIndexTriangleDictionary[i][j]);
        //         j--;
        //     }
        // }
        //
        // for (int i = 0; i < 4; i++)
        // {
        //     points.RemoveAt(0);
        // }
    }

    //根据l反转其两边的三角
    private Line FlipLine(Line l)
    {
        // if (lineTriangleDictionary[l].Count == 1)
        // {
        //     return l;
        // }
        List<int> indices = new List<int>();
        indices.Add(l.minpointIndex);
        indices.Add(l.maxpointIndex);
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                bool flag = true;
                for (int k = 0; k < indices.Count; k++)
                {
                    if (indices[k] == lineTriangleDictionary[l][i].verticesIndices[j])
                    {
                        flag = false;
                        break;
                    }
                }

                if (flag)
                {
                    indices.Add(lineTriangleDictionary[l][i].verticesIndices[j]);
                }
            }
        }
        RemoveFromMesh(lineTriangleDictionary[l][0]);
        //清除之后索引减少
        RemoveFromMesh(lineTriangleDictionary[l][0]);
        AddToMesh(new Triangle(indices[0], indices[2], indices[3], this));
        AddToMesh(new Triangle(indices[1], indices[2], indices[3], this));
                
        return new Line(indices[2], indices[3],this);
    }
    
    //约束Delaunay三角剖分,参数分别是:要添加的点,约束边,是否删除内部或外部三角
    public List<int> ConstrainedTriangulation(List<Vector2> newPoints, Delete delete = Delete.NO,
        List<Line> constrainedLines = null)
    {
        List<int> indices = AddNewPointsToIndices(newPoints);
        if (constrainedLines == null)
        {
            constrainedLines=ConstrainedLines(indices);
        }
        
        HashSet<int> newPointIndices = new HashSet<int>();


        for (int i = 0; i < indices.Count; i++)
        {
            if (delete!=Delete.NO)
            {
                newPointIndices.Add(indices[i]);
            }
            Triangulation(indices[i]);
        }
        Queue<Line> conflictLines = new Queue<Line>();
        for (int i = 0; i < constrainedLines.Count; i++)
        {
            if (!lines.Contains(constrainedLines[i]))
            {
                foreach (Line l in lines)
                {
                    if (constrainedLines[i].CheckLine(l))
                    {
                        conflictLines.Enqueue(l);
                    }
                }
                while (conflictLines.Count > 0)
                {
                    Line l=FlipLine(conflictLines.Peek());
                    if (constrainedLines[i].CheckLine(l))
                    {
                        conflictLines.Enqueue(l);
                    }
                    conflictLines.Dequeue();
                }
            }
        }

        if (delete!=Delete.NO)
        {
            HashSet<Triangle> outTriangles = new HashSet<Triangle>();
            HashSet<Triangle> inTriangles = new HashSet<Triangle>();
            HashSet<Triangle> allTriangles = new HashSet<Triangle>();
            
            Queue<Triangle> trianglesQueue = new Queue<Triangle>();
            foreach (int newpointIndex in newPointIndices)
            {
                for (int j = 0; j < pointIndexTriangleDictionary[newpointIndex].Count; j++)
                {
                    allTriangles.Add(pointIndexTriangleDictionary[newpointIndex][j]);
                }
            }
            
            
            foreach (Triangle triangle in allTriangles)
            {
                trianglesQueue.Enqueue(triangle);
                for (int j = 0; j < 3; j++)
                {
                    if (!newPointIndices.Contains(triangle.verticesIndices[j]))
                    {
                        outTriangles.Add(triangle);
                        break;
                    }
                }
            }
            Debug.Log(trianglesQueue.Count);
            
            while(trianglesQueue.Count > 0)
            {
                if (!outTriangles.Contains(trianglesQueue.Peek()) && !inTriangles.Contains(trianglesQueue.Peek()))
                {
                    bool flag = true;
                    for (int i = 0; i < 3; i++)
                    {
                        Line l = new Line(trianglesQueue.Peek().verticesIndices[i],
                            trianglesQueue.Peek().verticesIndices[(i + 1) % 3], this);
                        int tmp = 0;
                        for (; tmp < 2; tmp++)
                        {
                            if (lineTriangleDictionary[l][tmp].Equals(trianglesQueue.Peek()))
                            {
                                break;
                            }
                        }
                        if (outTriangles.Contains(lineTriangleDictionary[l][(tmp+1)%2]))
                        {
                            flag = false;
                            if (constrainedLines.Contains(l))
                            {
                                inTriangles.Add(trianglesQueue.Peek());
                                break;
                            }
                            outTriangles.Add(trianglesQueue.Peek());
                            break;
                        }
                        if (inTriangles.Contains(lineTriangleDictionary[l][(tmp+1)%2]))
                        {
                            flag = false;
                            if (constrainedLines.Contains(l))
                            {
                                outTriangles.Add(trianglesQueue.Peek());
                                break;
                            }
                            inTriangles.Add(trianglesQueue.Peek());
                            break;
                        }
                    }

                    if (flag)
                    {
                        trianglesQueue.Enqueue(trianglesQueue.Peek());
                    }
                    trianglesQueue.Dequeue();
                }
                else
                {
                    trianglesQueue.Dequeue();
                }
            }
            
            
            
            
            // foreach (Triangle triangle in allTriangles)
            // {
            //     if (!outTriangles.Contains(triangle) && !inTriangles.Contains(triangle))
            //     {
            //         for (int i = 0; i < 3; i++)
            //         {
            //             Line l = new Line(triangle.verticesIndices[i],
            //                 triangle.verticesIndices[(i + 1) % 3], this);
            //             int tmp = 0;
            //             for (; tmp < 2; tmp++)
            //             {
            //                 if (lineTriangleDictionary[l][tmp].Equals(triangle))
            //                 {
            //                     break;
            //                 }
            //             }
            //             if (outTriangles.Contains(lineTriangleDictionary[l][(tmp+1)%2]))
            //             {
            //                 if (constrainedLines.Contains(l))
            //                 {
            //                     inTriangles.Add(triangle);
            //                 }
            //                 else
            //                 {
            //                     outTriangles.Add(triangle);
            //                 }
            //             }
            //             else if (inTriangles.Contains(lineTriangleDictionary[l][(tmp+1)%2]))
            //             {
            //                 if (constrainedLines.Contains(l))
            //                 {
            //                     outTriangles.Add(triangle);
            //                 }
            //                 else
            //                 {
            //                     inTriangles.Add(triangle);
            //                 }
            //             }
            //         }
            //     }
            // }
            Debug.Log(outTriangles.Count);
            Debug.Log(inTriangles.Count);

            if (delete == Delete.OUT)
            {
                foreach (Triangle triangle in outTriangles)
                {
                    RemoveFromMesh(triangle);
                }
            }
            else if (delete == Delete.IN) 
            {
                foreach (Triangle triangle in inTriangles)
                {
                    RemoveFromMesh(triangle);
                }
            }
        }

        return indices;
    }
    
    //Delaunay三角剖分
    public void Triangulation(int pointIndex)
    {
        HashSet<int> hashIndices=new HashSet<int>();
        List<int> newPointsIndices=new List<int>();
        List<Triangle> deleteTriangles=new List<Triangle>();
        foreach (Triangle triangle in triangles)
        {
            if (triangle.circumcircle.Check(points[pointIndex]))
            {
                for (int j = 0; j < 3; j++)
                {
                    hashIndices.Add(triangle.verticesIndices[j]);
                }
                deleteTriangles.Add(triangle);
            }
        }

        for (int i = 0; i < deleteTriangles.Count; i++)
        {
            RemoveFromMesh(deleteTriangles[i]);
        }
        deleteTriangles.Clear();

        foreach (int vindices in hashIndices)
        {
            newPointsIndices.Add(vindices);
        }
        Clockwise(newPointsIndices,points[pointIndex]);
        
        for (int i = 0; i < newPointsIndices.Count; i++)
        {
            if (Vector3.Cross(points[newPointsIndices[i]] - points[newPointsIndices[(i + 1) % newPointsIndices.Count]],
                    points[newPointsIndices[(i + 1) % newPointsIndices.Count]] - points[pointIndex]).z > 0.01f)
            {
                AddToMesh(new Triangle(newPointsIndices[i], newPointsIndices[(i + 1) % newPointsIndices.Count],
                    pointIndex, this));
            }
        }
    }
    
    //耳切法,可能失效了
    // public void EarCut()
    // {
    //     List<int> handlePoints = new List<int>();
    //     for (int i = 0; i < points.Count; i++)
    //     {
    //         handlePoints.Add(i);
    //     }
    //     for (int i = 0; handlePoints.Count>2; i++)
    //     {
    //         i%=handlePoints.Count;
    //         Vector2 a=points[handlePoints[i]];
    //         Vector2 b=points[handlePoints[(i+1)%handlePoints.Count]];
    //         Vector2 c=points[handlePoints[(i+2)%handlePoints.Count]];
    //         if (Vector3.Cross(b - a, c - b).z < 0)
    //         {
    //             continue;
    //         }
    //
    //         Triangle triangle = new Triangle(handlePoints[i], handlePoints[(i + 1) % handlePoints.Count],
    //             handlePoints[(i + 2) % handlePoints.Count], this);
    //         bool flag = true;
    //         for (int j = i + 3; j%handlePoints.Count != i; j++)
    //         {
    //             j%=handlePoints.Count;
    //             if (triangle.Check(points[handlePoints[j%handlePoints.Count]]))
    //             {
    //                 flag = false;
    //                 break;
    //             }
    //         }                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     
    //         if (flag)
    //         {
    //             AddToMesh(triangle);
    //             handlePoints.RemoveAt((i+1)%handlePoints.Count);
    //             i--;
    //         }
    //     }
    // }

    // public void DeletePoint(int pointIndex)
    // {
    //     HashSet<int> hashIndices=new HashSet<int>();
    //     List<int> emptyHolePointsIndices=new List<int>();
    //     for (int i = 0; i < pointIndexTriangleDictionary[pointIndex].Count; i++)
    //     {
    //         for (int j = 0; j < 3; j++)
    //         {
    //             hashIndices.Add(pointIndexTriangleDictionary[pointIndex][i].verticesIndices[j]);
    //         }
    //         RemoveFromMesh(pointIndexTriangleDictionary[pointIndex][i]);
    //         i--;
    //     }
    //     hashIndices.Remove(pointIndex);
    //     foreach (int vindices in hashIndices)
    //     {
    //         emptyHolePointsIndices.Add(vindices);
    //     }
    //     EmptyHoleTriangulation(emptyHolePointsIndices);
    // }
    //
    // void EmptyHoleTriangulation(List<int> pointsIndices)
    // {
    //     Vector2 leftdown=points[pointsIndices[0]], rightup=points[pointsIndices[0]];
    //     for (int i = 0; i < pointsIndices.Count; i++)
    //     {
    //         leftdown = Vector2.Min((points[pointsIndices[i]]), leftdown);
    //         rightup = Vector2.Max((points[pointsIndices[i]]), rightup);
    //     }
    //     leftdown-=Vector2.one;
    //     rightup+=Vector2.one;
    //     List<Vector2> tmpPoints = new List<Vector2>();
    //     tmpPoints.Add(leftdown);
    //     tmpPoints.Add(new Vector2(rightup.x, leftdown.y));
    //     tmpPoints.Add(rightup);
    //     tmpPoints.Add(new Vector2(leftdown.x, rightup.y));
    //     List<Triangle> triangles = new List<Triangle>();
    //     Triangle triangle1 = new Triangle(0,1,2,this);
    //     Triangle triangle2 = new Triangle(0,2,3,this);
    //     triangles.Add(triangle1);
    //     triangles.Add(triangle2);
    //     
    //     for (int i = 0; i < pointsIndices.Count; i++)
    //     {
    //         tmpPoints.Add(points[pointsIndices[i]]);
    //         SeparatedTriangulation(tmpPoints,triangles,i+4);
    //     }
    //
    //     for (int i = 0; i < triangles.Count; i++)
    //     {
    //         bool flag = true;
    //         for (int j = 0; j < 3; j++)
    //         {
    //             if (triangles[i].verticesIndices[j] < 4)
    //             {
    //                 flag = false;
    //                 break;
    //             }
    //         }
    //
    //         if (flag)
    //         {
    //             InstantiateTriangleMesh(pointsIndices[triangles[i].verticesIndices[0] - 4],
    //                 pointsIndices[triangles[i].verticesIndices[1] - 4],
    //                 pointsIndices[triangles[i].verticesIndices[2] - 4]);
    //         }
    //     }
    // }
    //
    // void SeparatedTriangulation(List<Vector2> points,List<Triangle> triangles, int pointIndex)
    // {
    //     HashSet<int> hashIndices=new HashSet<int>();
    //     List<int> newPointsIndices=new List<int>();
    //     for (int i = 0; i < triangles.Count; i++)
    //     {
    //         if (triangles[i].circumcircle.Check(points[pointIndex]))
    //         {
    //             for (int j = 0; j < 3; j++)
    //             {
    //                 hashIndices.Add(triangles[i].verticesIndices[j]);
    //             }
    //             triangles.RemoveAt(i);
    //             i--;
    //         }
    //     }
    //
    //     foreach (int vindices in hashIndices)
    //     {
    //         newPointsIndices.Add(vindices);
    //     }
    //     
    //     Clockwise(points,newPointsIndices,pointIndex);
    //     
    //     for (int i = 0; i < newPointsIndices.Count; i++)
    //     {
    //         if (Vector3.Cross(points[newPointsIndices[i]] - points[newPointsIndices[(i + 1) % newPointsIndices.Count]],
    //                 points[newPointsIndices[(i + 1) % newPointsIndices.Count]] - points[pointIndex]).z > 0.01f)
    //         {
    //             Triangle triangle = new Triangle(newPointsIndices[i],
    //                 newPointsIndices[(i + 1) % newPointsIndices.Count],
    //                 pointIndex, points);
    //             triangles.Add(triangle);
    //         }
    //     }
    // }
    
    //将三角添加到网格中
    public void AddToMesh(Triangle triangle)
    {
        triangles.Add(triangle);
        ConnectLine(triangle);
        ConnectPoint(triangle);
    }

    //将三角移除网格
    public void RemoveFromMesh(Triangle triangle)
    {
        triangles.Remove(triangle);
        DisConnectLine(triangle);
        DisConnectPoint(triangle);
    }
    
    //将三角与其三条边相关联
    void ConnectLine(Triangle triangle)
    {
        for (int i = 0; i < 3; i++)
        {
            Line l=new Line(triangle.verticesIndices[i], triangle.verticesIndices[(i + 1) % 3],this);
            lines.Add(l);
            if (!lineTriangleDictionary.ContainsKey(l))
            {
                List<Triangle> tmp = new List<Triangle>(){triangle};
                lineTriangleDictionary.Add(l,tmp);
            }
            else
            {
                lineTriangleDictionary[l].Add(triangle);
            }
        }
    }

    //将三角与其三个点相关联
    void ConnectPoint(Triangle triangle)
    {
        for (int i = 0; i < 3; i++)
        {
            if (!pointIndexTriangleDictionary.ContainsKey(triangle.verticesIndices[i]))
            {
                List<Triangle> tmp = new List<Triangle>(){triangle};
                pointIndexTriangleDictionary.Add(triangle.verticesIndices[i],tmp);
            }
            else
            {
                pointIndexTriangleDictionary[triangle.verticesIndices[i]].Add(triangle);
            }
        }
    }

    //将三角与其三条边移除关联
    void DisConnectLine(Triangle triangle)
    {
        for (int i = 0; i < 3; i++)
        {
            Line l=new Line(triangle.verticesIndices[i], triangle.verticesIndices[(i + 1) % 3],this);
            lineTriangleDictionary[l].Remove(triangle);
            if (lineTriangleDictionary[l].Count==0)
            {
                lineTriangleDictionary.Remove(l);
                lines.Remove(l);
            }
        }
    }

    //将三角与其三个点移除关联
    void DisConnectPoint(Triangle triangle)
    {
        for (int i = 0; i < 3; i++)
        {
            pointIndexTriangleDictionary[triangle.verticesIndices[i]].Remove(triangle);
        }
    }
}
