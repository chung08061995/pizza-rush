using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Presentation-only assembly for the four production slots that form one
/// board cell. Gameplay occupancy remains owned by ContainerPlace.
/// </summary>
public sealed class PizzaQuarterAssemblyVisual : MonoBehaviour
{
    private const float LandingHeight = 0.18f;
    private const float CompletedInset = 0.006f;
    private const float SnapDuration = 0.18f;

    private readonly HashSet<ContainerPlace> landedPlaces = new();
    private ContainerPlaces owner;
    private bool completed;

    public void Initialize(ContainerPlaces containerPlaces)
    {
        owner = containerPlaces;
        landedPlaces.Clear();
        completed = false;
    }

    public int GetQuarterIndex(ContainerPlace place)
    {
        return owner == null || place == null ? -1 : owner.Places.IndexOf(place);
    }

    public void NotifyAssigned(ContainerPlace place)
    {
        if (place == null)
        {
            return;
        }
        landedPlaces.Remove(place);
        if (completed)
        {
            ResetVisuals();
        }
    }

    public void NotifyLanded(ContainerPlace place, bool animate)
    {
        if (owner == null || place == null || place.Production == null)
        {
            return;
        }

        landedPlaces.Add(place);
        if (IsAssemblyComplete())
        {
            CompleteAssembly(animate);
        }
        else
        {
            SetPartialPose(place);
        }
    }

    public void NotifyRemoved(ContainerPlace place)
    {
        landedPlaces.Remove(place);
        completed = false;
        ResetVisuals();
    }

    public void RefreshImmediate()
    {
        if (owner == null)
        {
            return;
        }

        landedPlaces.Clear();
        foreach (ContainerPlace place in owner.Places)
        {
            if (place != null && place.Production != null)
            {
                landedPlaces.Add(place);
            }
        }

        if (IsAssemblyComplete())
        {
            CompleteAssembly(false);
        }
        else
        {
            completed = false;
            ResetVisuals();
        }
    }

    private bool IsAssemblyComplete()
    {
        return owner != null &&
               owner.Places.Count == 4 &&
               owner.Places.All(place =>
                   place != null &&
                   place.Production != null &&
                   landedPlaces.Contains(place));
    }

    private void CompleteAssembly(bool animate)
    {
        completed = true;
        foreach (ContainerPlace place in owner.Places)
        {
            Production production = place.Production;
            if (production == null)
            {
                continue;
            }

            production.transform.DOKill();
            production.QuarterVisual?.SetAssemblyMode(true);
            Vector3 target = GetAssemblyLocalPosition(place, CompletedInset);
            Quaternion targetRotation = GetQuarterRotation(place);
            if (!animate)
            {
                production.transform.localPosition = target;
                production.transform.localRotation = targetRotation;
                production.transform.localScale = Vector3.one;
                production.QuarterVisual?.SetCompletionFlash(0f);
                continue;
            }

            production.transform
                .DOLocalMove(target, SnapDuration)
                .SetEase(Ease.OutBack)
                .SetTarget(production.transform);
            production.transform
                .DOLocalRotateQuaternion(targetRotation, SnapDuration)
                .SetEase(Ease.OutBack)
                .SetTarget(production.transform);
            production.transform
                .DOPunchScale(Vector3.one * 0.055f, 0.24f, 2, 0.35f)
                .SetEase(Ease.OutQuad)
                .SetTarget(production.transform);

            PizzaQuarterVisual quarterVisual = production.QuarterVisual;
            if (quarterVisual != null)
            {
                DOVirtual.Float(0f, 1f, 0.08f, quarterVisual.SetCompletionFlash)
                    .SetTarget(production.transform)
                    .OnComplete(() =>
                    {
                        DOVirtual.Float(1f, 0f, 0.16f, quarterVisual.SetCompletionFlash)
                            .SetTarget(production.transform);
                    });
            }
        }
    }

    private void SetPartialPose(ContainerPlace place)
    {
        Production production = place == null ? null : place.Production;
        if (production == null)
        {
            return;
        }
        production.transform.DOKill();
        production.QuarterVisual?.SetAssemblyMode(true);
        production.transform.localPosition = GetAssemblyLocalPosition(place, 0f);
        production.transform.localRotation = GetQuarterRotation(place);
        production.transform.localScale = Vector3.one;
        production.QuarterVisual?.SetCompletionFlash(0f);
    }

    private Vector3 GetAssemblyLocalPosition(ContainerPlace place, float inset)
    {
        if (place == null || place.Pizza == null)
        {
            return new Vector3(inset, LandingHeight, inset);
        }

        // The four Pizza anchors have different local rotations. Calculate the
        // shared centre in world space, then convert it into this anchor's
        // local space; negating localPosition alone is wrong when an anchor is
        // rotated (it was the source of the diagonal/off-cell pieces).
        Vector3 center = GetCellCenterWorld();
        center += owner.transform.up * LandingHeight;
        center += owner.transform.right * inset;
        center += owner.transform.forward * inset;
        return place.Pizza.InverseTransformPoint(center);
    }

    private Quaternion GetQuarterRotation(ContainerPlace place)
    {
        if (place == null || place.Pizza == null)
        {
            return Quaternion.identity;
        }

        Vector3 worldDelta = place.Pizza.position - GetCellCenterWorld();
        Vector3 delta = owner.transform.InverseTransformDirection(worldDelta);
        float angle;
        if (delta.x >= 0f)
        {
            angle = delta.z >= 0f ? 180f : 270f;
        }
        else
        {
            angle = delta.z >= 0f ? 90f : 0f;
        }

        Quaternion worldRotation = owner.transform.rotation * Quaternion.Euler(0f, angle, 0f);
        return Quaternion.Inverse(place.Pizza.rotation) * worldRotation;
    }

    private Vector3 GetCellCenterWorld()
    {
        if (owner == null || owner.Places == null || owner.Places.Count == 0)
        {
            return transform.position;
        }

        Vector3 center = Vector3.zero;
        int count = 0;
        foreach (ContainerPlace place in owner.Places)
        {
            if (place == null || place.Pizza == null)
            {
                continue;
            }
            center += place.Pizza.position;
            count++;
        }
        return count == 0 ? transform.position : center / count;
    }

    private void ResetVisuals()
    {
        if (owner == null)
        {
            return;
        }

        foreach (ContainerPlace place in owner.Places)
        {
            if (place == null || place.Production == null)
            {
                continue;
            }
            SetPartialPose(place);
        }
    }

    private void OnDisable()
    {
        if (owner != null)
        {
            foreach (ContainerPlace place in owner.Places)
            {
                if (place == null || place.Production == null)
                {
                    continue;
                }
                place.Production.transform.DOKill();
                place.Production.QuarterVisual?.SetCompletionFlash(0f);
            }
        }
        landedPlaces.Clear();
        completed = false;
    }
}
