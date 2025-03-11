using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    const float DURATION = 1f;

    int segmentsPerSegment = 10;
    int bifurcationSegments = 4;
    float offsetRange = 0.25f;
    float bifurcationChance = 0.3f;
    float bifurcationOffsetRange = 0.3f;



    [SerializeField] LineRenderer OuterLine;
    [SerializeField] LineRenderer InnerLine;

    List<Vector3> _positions;

    float _chrono = 0f;

    public void Init(List<Vector3> positions, bool direct = false)
    {
        _positions = positions;
        if(!direct)
        {
            DrawLightningBifurcations();
        }
        else
        {
            DrawLighnintDirect();
        }
    }

    private void Update()
    {
        _chrono += Time.deltaTime;
        Color outerColor = OuterLine.startColor;
        outerColor.a = 1 - (_chrono / DURATION);
        OuterLine.startColor = outerColor;
        OuterLine.endColor = outerColor;

        Color innerColor = InnerLine.startColor;
        innerColor.a = 1 - (_chrono / DURATION);
        InnerLine.startColor = innerColor;
        InnerLine.endColor = innerColor;

        if(_chrono >= DURATION)
        {
            Destroy(gameObject);
        }
    }

    void DrawLightningBifurcations()
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < _positions.Count - 1; i++)
        {
            Vector3 start = _positions[i];
            Vector3 end = _positions[i + 1];
            AddLightningSegment(positions, start, end);
        }

        OuterLine.positionCount = positions.Count;
        OuterLine.SetPositions(positions.ToArray());

        InnerLine.positionCount = positions.Count;
        InnerLine.SetPositions(positions.ToArray());
    }

    void DrawLighnintDirect()
    {
        OuterLine.positionCount = _positions.Count;
        OuterLine.SetPositions(_positions.ToArray());

        InnerLine.positionCount = _positions.Count;
        InnerLine.SetPositions(_positions.ToArray());
    }

    void AddLightningSegment(List<Vector3> positions, Vector3 start, Vector3 end)
    {
        positions.Add(start);
        Vector3 lastPosition = start;

        for (int j = 1; j <= segmentsPerSegment; j++)
        {
            float t = (float)j / segmentsPerSegment;
            Vector3 newPosition = Vector3.Lerp(start, end, t);
            newPosition += RandomOffset(offsetRange);

            if (Random.value < bifurcationChance)
            {
                CreateBifurcation(lastPosition, newPosition);
            }

            positions.Add(newPosition);
            lastPosition = newPosition;
        }
    }

    void CreateBifurcation(Vector3 start, Vector3 end)
    {
        GameObject bifurcation = Instantiate(gameObject);
        Lightning lightning = bifurcation.GetComponent<Lightning>();

        List<Vector3> bifurcationPositions = new List<Vector3> { start };

        for (int k = 1; k <= bifurcationSegments; k++)
        {
            float t = (float)k / bifurcationSegments;
            Vector3 bifurcationPos = Vector3.Lerp(start, end, t);
            bifurcationPos += RandomOffset(bifurcationOffsetRange);
            bifurcationPositions.Add(bifurcationPos);
        }

        lightning.Init(bifurcationPositions, true);
    }

    Vector3 RandomOffset(float range)
    {
        return new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
    }
}
