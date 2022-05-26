using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Control : MonoBehaviour
{
    private List<GameObject> points = new List<GameObject>();
    private GameObject targetObject;

    private Vector3 formerPos = Vector3.zero;

    public const int updateSteps = 100;

    private LineRenderer lineRenderer;

    private void Init(int numPoints)
    {
        Vector3 nowVector = Vector3.zero;
        Vector3 localVector = Vector3.up * 10f;
        for(int i=0; i<numPoints; i++)
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            points.Add(gameObject);
            if(i > 0)
            {
                gameObject.transform.SetParent(points[i - 1].transform);
                gameObject.transform.localPosition = localVector;
            }
        }
    }

    private (float, float, float) ComputeGlobalAngle(Vector3 direction)
    {
        float xangle, yangle, zangle;

        if (direction.y != 0)
        {
            xangle = Mathf.Atan(direction.z / direction.y);
        }
        else
        {
            if (direction.z > 0)
            {
                xangle = Mathf.PI / 2;
            }
            else
            {
                xangle = -Mathf.PI / 2;
            }
        }

        if (direction.z != 0)
        {
            yangle = Mathf.Atan(direction.x / direction.z);
        }
        else
        {
            if (direction.x > 0)
            {
                yangle = Mathf.PI / 2;
            }
            else
            {
                yangle = -Mathf.PI / 2;
            }
        }

        if (direction.x != 0)
        {
            zangle = Mathf.Atan(direction.y / direction.x);
        }
        else
        {
            if (direction.y > 0)
            {
                zangle = Mathf.PI / 2;
            }
            else
            {
                zangle = -Mathf.PI / 2;
            }
        }

        return (xangle, yangle, zangle);
    }

    private void UpdateIKStep()
    {
        GameObject endObject = points[points.Count - 1];
        for(int i=points.Count-2; i>=0; i--)
        {
            GameObject nowObject = points[i];
            Vector3 nowVector = endObject.transform.position - nowObject.transform.position;
            Vector3 targetVector = targetObject.transform.position - nowObject.transform.position;
            Vector3 crossProduct = Vector3.Cross(nowVector, targetVector);
            float degBetween = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(nowVector.normalized, targetVector.normalized));

            if (nowVector.magnitude < 10e-6f || targetVector.magnitude < 10e-6f || crossProduct.magnitude == 0f || float.IsNaN(degBetween)) 
            {
                return;
            }
            Quaternion qua = Quaternion.AngleAxis(degBetween, crossProduct);

            nowObject.transform.localRotation *= qua;
        }
    }

    private void UpdateIK()
    {
        for(int i = 0; i < updateSteps; i++)
        {
            UpdateIKStep();
        }
    }

    private void UpdateLine()
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.Select(x => x.transform.position).ToArray());
    }

    // Start is called before the first frame update
    void Start()
    {
        targetObject = GameObject.FindGameObjectWithTag("target");
        Init(4);
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
        UpdateIK();
        UpdateLine();
    }



    // Update is called once per frame
    void Update()
    {
        Vector3 target = targetObject.transform.position;
        if(formerPos != target)
        {
            UpdateIK();
            UpdateLine();
            formerPos = target;
        }
        
    }
}
