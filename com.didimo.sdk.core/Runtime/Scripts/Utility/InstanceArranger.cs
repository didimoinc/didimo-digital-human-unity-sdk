using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



public class ShapeVertex
{
    public ShapeVertex(Vector3 p, Vector3 t, Vector2 uv_)
    {
        position = p;
        tangent = t;
        uv = uv_;
    }
    public Vector2 position;
    public Vector2 tangent;
    public Vector2 uv;    
}

public interface IShapeGenerator2D
{
    public ShapeVertex GetVertex(float dx);       
}

public class LineShape: IShapeGenerator2D
{
    Vector2 a, b, d;
    public LineShape(Vector2 a, Vector2 b)
    {
        this.a = a;
        this.b = b;
        this.d = b - a;
    }

    public ShapeVertex GetVertex(float cx)
    {     
        return new ShapeVertex(a + d * cx, new Vector2(d.y, -d.x), new Vector2(cx, 0.0f));
    }
}

public class SquareShape: IShapeGenerator2D
{
    Vector2 a, b, d;
    public SquareShape(Vector2 a, Vector2 b)
    {
        this.a = a;
        this.b = b;
        this.d= b - a;
    }
    public Vector2 GetPos(int side, float ldx)
    {
        switch (side)
        {
            case 0: return new Vector2(a.x + d.x * ldx, a.y);
            case 1: return new Vector2(b.x, a.y + d.y * ldx);
            case 2: return new Vector2(b.x - d.x * ldx, b.y);
            case 3: return new Vector2(a.x, b.y - d.y * ldx);
            default: return Vector2.zero;
        }
    }
    static Vector2[] tangents = { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) };

    public ShapeVertex GetVertex(float cx)
    {
        int side = Mathf.FloorToInt(cx * 4.0f);
        float ldx = ((cx * 4.0f) - side);
        Vector2 pos = GetPos(side, ldx);
        return new ShapeVertex(pos, tangents[side], new Vector2(cx, 0));        
    }
}

public class CircleShape: IShapeGenerator2D
{
    Vector2 c;
    float r;
    public CircleShape(Vector2 c, float r)
    {
        this.c = c;
        this.r = r;
    }
    
    public ShapeVertex GetVertex(float cx)
    {
        Vector2 v = new Vector2(Mathf.Cos(cx * 2.0f * Mathf.PI), Mathf.Sin(cx * 2.0f * Mathf.PI));
        Vector2 pt = c + v * r;
        return new ShapeVertex(pt, v, new Vector2(cx, 0.0f));
    }
}

public class GridShape: IShapeGenerator2D
{
    Vector2 a, b, d;
    int xcount;
    int ycount;
    int total;
    float rowlength;
    public GridShape(Vector2 a, Vector2 b, int xcount, int ycount)
    {
        this.a = a;
        this.b = b;
        this.d = b - a;
        this.xcount = xcount;
        this.ycount = ycount;
        total = xcount * ycount;
        this.rowlength = 1.0f / xcount;
    }

    public ShapeVertex GetVertex(float cx)
    {
        
        float xf = (float)xcount;
        //float offs = ((xcount & 1) == 0) ? -0.125f : 0;
        float offs = -1.0f / (xcount * 2.0f);// -0.125f : 0;

        float xp = (cx % rowlength * xf) - offs;
        float yp = (Mathf.Floor(cx / rowlength) / xf) -offs;



        return new ShapeVertex(new Vector2(a.x + offs + xp * d.x, a.y +offs + yp * d.y), new Vector2(0,0), new Vector2(xp,yp));
    }
}

public class ShapeEnumerator : IEnumerable<ShapeVertex>
{
    public int Steps = 1;
    IShapeGenerator2D shape;

    public float DX
    {
        get { return 1.0f / Steps;}
    }

    public ShapeEnumerator(IShapeGenerator2D shape_, int steps_ = 1)
    {
        Steps = steps_;
        shape = shape_;
    }

    public class ShapeEnum: IEnumerator<ShapeVertex>
    {
        ShapeEnumerator parent;
        float cx = 0.0f;
        public ShapeEnum(ShapeEnumerator parent, float cx)
        {
            this.cx = cx;
            this.parent = parent;
        }
        object IEnumerator.Current
        {
            get{return Current;}
        }

        public ShapeVertex Current
        {
            get
            {                
                return parent.shape.GetVertex(cx);
            }
        }

        public bool MoveNext() { if (cx < 1.0f) { cx += parent.DX; return true; } else { return false; } }
        public void Reset(){cx = -parent.DX;}
        public void Dispose(){}
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return new ShapeEnum(this, -DX);
    }

    public IEnumerator<ShapeVertex> GetEnumerator()
    {
        return new ShapeEnum(this, -DX);
    }
}



public class InstanceArranger : MonoBehaviour
{
    public enum ShapeType
    {
        Circle,
        Square,
        Grid,
        Line
    }

    IEnumerable<ShapeVertex> generator;

    public ShapeType Shape;
   
    [Range(-100.0f, 100.0f)]
    public float Scale = 1.0f;

    public Quaternion Orientation = new Quaternion();
    public Quaternion LocalOrientation = new Quaternion();

    Vector2 RotateVector(Vector2 inVec, float angle)
    {
        float cr = Mathf.Cos(angle);
        float sr = Mathf.Sin(angle);
        return new Vector2(inVec.x * cr + inVec.y * sr, inVec.x * sr + inVec.y * -cr);
    }

    void Calculate()
    {
        List<Transform> obs= new List<Transform>();
        for (var i = 0; i < gameObject.transform.childCount; ++i)
        {            
            obs.Add(gameObject.transform.GetChild(i).transform);
        }
        int sqrtcount = (int)Mathf.Sqrt(obs.Count);
        IShapeGenerator2D shapeGen = null;
        switch (Shape)
        {
            case ShapeType.Circle: shapeGen = new CircleShape(Vector2.zero, Scale);break;
            case ShapeType.Line:
                {
                    Vector2 lv = new Vector2(Scale, 0.0f);
                    shapeGen = new LineShape(-lv, lv); break;
                }
            case ShapeType.Grid: shapeGen = new GridShape(new Vector2(-Scale * 0.5f, -Scale * 0.5f), new Vector2(Scale * 0.5f, Scale * 0.5f), sqrtcount, sqrtcount); break;
            case ShapeType.Square: shapeGen = new SquareShape(new Vector2(-Scale * 0.5f, -Scale * 0.5f), new Vector2(Scale * 0.5f, Scale * 0.5f)); break;
        }
        generator = new ShapeEnumerator(shapeGen, obs.Count);        
        foreach (var tuple in generator.Zip(obs, (pt,inst)=>(pt,inst)))
        {
            var v3 = new Vector3(tuple.pt.position.x, tuple.pt.position.y, 0);
            var t3 = new Vector3(tuple.pt.tangent.x, tuple.pt.tangent.y, 0);
            
            tuple.inst.position = Orientation * v3;
            var up = new Vector3(0, 0, -1);            
            tuple.inst.localRotation = Orientation * Quaternion.LookRotation(t3, up) * LocalOrientation;
        }

    }

    void OnValidate()
    {
        Calculate();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
