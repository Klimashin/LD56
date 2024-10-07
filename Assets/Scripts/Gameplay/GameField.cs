using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameField : MonoBehaviour
{
    public Floor[] Floors;

    public Vector2 GetSlotPosition(CharacterPosition characterPosition)
    {
        return characterPosition.zoneType == ZoneType.Left
            ? Floors[characterPosition.floorIndex].LeftZone.GetSlotPosition(characterPosition.zoneIndex)
            : Floors[characterPosition.floorIndex].RightZone.GetSlotPosition(characterPosition.zoneIndex);
    }

    public ZoneHitData GetZoneHitData(Vector2 hitPos)
    {
        for (int floorIndex = 0; floorIndex < Floors.Length; floorIndex++)
        {
            var floor = Floors[floorIndex];
            var leftZone = floor.LeftZone;
            for (var slotRectIndex = 0; slotRectIndex < leftZone.slotRects.Length; slotRectIndex++)
            {
                if (leftZone.slotRects[slotRectIndex].Contains(hitPos))
                {
                    return new ZoneHitData(true, ZoneType.Left, floorIndex, slotRectIndex);
                }
            }

            var rightZone = floor.RightZone;
            for (var slotRectIndex = 0; slotRectIndex < rightZone.slotRects.Length; slotRectIndex++)
            {
                if (rightZone.slotRects[slotRectIndex].Contains(hitPos))
                {
                    return new ZoneHitData(true, ZoneType.Right, floorIndex, slotRectIndex);
                }
            }
        }

        return new ZoneHitData(false);
    }

    private void OnDrawGizmos()
    {
        for (var i = 0; i < Floors.Length; i++)
        {
            if (Floors[i] == null || Floors[i].FloorBottomCenter == null)
            {
                continue;
            }
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            var leftZone = Floors[i].LeftZone;
            var rightZone = Floors[i].RightZone;
            Gizmos.DrawCube(leftZone.zoneRect.center, leftZone.zoneRect.size);
            Gizmos.DrawCube(rightZone.zoneRect.center, rightZone.zoneRect.size);

            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            for (int j = 0; j < leftZone.slotsCount; j++)
            {
                Gizmos.DrawSphere(leftZone.GetSlotPosition(j), 0.2f);
                Gizmos.DrawSphere(rightZone.GetSlotPosition(j), 0.2f);
            }
        }
    }
}

public enum ZoneType
{
    Left,
    Right
}

[Serializable]
public class Floor
{
    public const int FLOOR_SLOTS_COUNT = 4;
    
    public Transform FloorBottomCenter;
    public Vector2 ZoneSize;
    public float ZoneOffsetFromCenter;
    public float ZoneSlotPointVerticalOffset;

    public ZoneData LeftZone => new ZoneData(
        (Vector2)FloorBottomCenter.position - new Vector2(ZoneOffsetFromCenter + ZoneSize.x, 0f),
        ZoneSize, 
        FLOOR_SLOTS_COUNT, 
        ZoneSlotPointVerticalOffset,
        ZoneType.Left);
    public ZoneData RightZone => new ZoneData(
        (Vector2)FloorBottomCenter.position + new Vector2(ZoneOffsetFromCenter, 0f),
        ZoneSize,
        FLOOR_SLOTS_COUNT,
        ZoneSlotPointVerticalOffset,
        ZoneType.Right);
}

public struct ZoneData
{
    public Rect zoneRect;
    public int slotsCount;
    public float slotPositionVerticalOffset;

    private ZoneType _zoneType;
    public Rect[] slotRects;

    public ZoneData(Vector2 leftCorner, Vector2 zoneSize, int slotsCount, float slotPositionVerticalOffset, ZoneType zoneType)
    {
        this.zoneRect = new Rect(leftCorner, zoneSize);
        this.slotsCount = slotsCount;
        this.slotPositionVerticalOffset = slotPositionVerticalOffset;
        this._zoneType = zoneType;
        this.slotRects = new Rect[slotsCount];

        this.CalcSlotsPositions();
    }

    private void CalcSlotsPositions()
    {
        for (int i = 0; i < slotsCount; i++)
        {
            var slotWidth = (zoneRect.xMax - zoneRect.xMin) / slotsCount;
            var slotHeight = zoneRect.yMax - zoneRect.yMin;
            var slotLeftCorner = _zoneType == ZoneType.Right
                ? new Vector2(zoneRect.xMin + slotWidth * i, zoneRect.yMin) 
                : new Vector2(zoneRect.xMax - slotWidth * (i + 1), zoneRect.yMin);

            slotRects[i] = new Rect(slotLeftCorner.x, slotLeftCorner.y, slotWidth, slotHeight);
        }
    }

    public Vector2 GetSlotPosition(int slotIndex)
    {
        UnityEngine.Assertions.Assert.IsTrue(slotIndex < slotsCount, "slotIndex is out of range");
        return slotRects[slotIndex].center - new Vector2(0f, slotPositionVerticalOffset);
    }
}

public struct ZoneHitData
{
    public bool zoneHit;
    public int floorIndex;
    public int zoneIndex;
    public ZoneType zoneType;

    public ZoneHitData(bool zoneHit, ZoneType zoneType, int floorIndex, int zoneIndex)
    {
        this.zoneHit = true;
        this.zoneIndex = zoneIndex;
        this.floorIndex = floorIndex;
        this.zoneType = zoneType;
    }
    
    public ZoneHitData(bool zoneHit)
    {
        this.zoneHit = false;
        this.zoneIndex = -1;
        this.floorIndex = -1;
        this.zoneType = ZoneType.Right;
    }

    public override string ToString()
    {
        return zoneHit ? $"Hit: true; floorIndex {floorIndex}; zoneType: {zoneType}; zoneIndex: {zoneIndex};" : "Hit: false;";
    }
}
