
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class TentacleTrap : MonoBehaviour
{
    private static readonly int DisappearAnimID = Animator.StringToHash("Disappear");
    [SerializeField] private Material spriteMaterial;
    [SerializeField] private Sprite ballSprite;
    [SerializeField] private Sprite tendrilSprite;

    [Min(0)] [SerializeField] private int BallCount;
    [SerializeField] private float SwaySpeed;
    [SerializeField] private float SwayAmount;
    [SerializeField] private float SwayOffset;
    [SerializeField] private float SwayToppleFactor;

    private RenderParams matRenderParms;
    private SpriteParams ballSpriteParms;
    private SpriteParams tendrilSpriteParms;

    private float swayTime;
    private bool disappearing;

    private Collider2D collider;
    [SerializeField] private ContactFilter2D CheckFilter;

    [NonSerialized] public PlayerBase owner;
    [NonSerialized] public bool visualOnly;
    
    private void Start()
    {
        PrepTendril();
        collider = GetComponent<Collider2D>();
    }

    private void PrepTendril()
    {
        matRenderParms = new RenderParams(spriteMaterial);
        ballSpriteParms = new SpriteParams(ballSprite);
        tendrilSpriteParms = new SpriteParams(tendrilSprite);
    }

    private float GetSegmentXOffset(int i)
    {
        float swayAmount = (swayTime - (i * SwayOffset));
        swayAmount = Mathf.Sin(swayAmount) * SwayAmount * Mathf.Pow(i, SwayToppleFactor);

        return swayAmount;
    }

    private float GetSegmentRot(int i)
    {
        if (i <= 0)
            return 0;
        var myPoint = new Vector2(GetSegmentXOffset(i), GetSegmentYOffset(i));
        var lastPoint = new Vector2(GetSegmentXOffset(i-1), GetSegmentYOffset(i-1));
        Vector2 dir = lastPoint - myPoint;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        return angle + 90.0f;
    }

    private float GetSegmentYOffset(int i)
    {
        return i * ballSprite.rect.height / ballSprite.pixelsPerUnit;
    }

    private Vector3 GetSegmentPosition(int i)
    {
        return new Vector3(GetSegmentXOffset(i), GetSegmentYOffset(i), 0);
    }

    private Matrix4x4 GetSegmentMatrix(int i)
    {
        return transform.localToWorldMatrix * Matrix4x4.Translate(GetSegmentPosition(i));
    }

    private Matrix4x4 GetSegmentMatrixRotated(int i)
    {
        return transform.localToWorldMatrix *  Matrix4x4.TRS(GetSegmentPosition(i), Quaternion.Euler(0, 0, GetSegmentRot(i)), Vector3.one);
    }

    private void DrawTendril()
    {
        if (BallCount > 0)
        {
            var matrices = new Matrix4x4[BallCount];
            for (int i = 0; i < matrices.Length; i++)
            {
                matrices[i] = GetSegmentMatrix(i);
            }
            Graphics.RenderSpriteInstanced(matRenderParms, ballSpriteParms, 0, matrices);
        }
        Graphics.RenderSprite(matRenderParms, tendrilSpriteParms, 0, GetSegmentMatrixRotated(BallCount));
    }

    private void Update()
    {
        swayTime += SwaySpeed * Time.deltaTime;
        DrawTendril();
    
        if(!visualOnly)
            CheckForStuff();
    }

    private void CheckForStuff()
    {
        if (!owner)
            owner = FindAnyObjectByType<PlayerMonster>();
        
        var arr = new Collider2D[5];
        collider.Overlap(CheckFilter, arr);

        for (int i = 0; i < arr.Length; i++)
        {
            if (!arr[i])
                break;

            var human = arr[i].GetComponent<KillableHuman>();
            if (human)
            {
                owner.KillHuman(human);
                Disappear();
                continue;
            }

            var rat = arr[i].GetComponent<PlayerRat>();
            if (rat)
            {
                rat.RatKill();
                Disappear();
                continue;
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void Disappear()
    {
        disappearing = true;
        GetComponent<Animator>().SetTrigger(DisappearAnimID);
    }

    public void DoneDisappearingEvent()
    {
        if (!disappearing)
            return;
        Destroy(gameObject);
    }

    
}
