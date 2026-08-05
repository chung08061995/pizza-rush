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

        // Pizza is a quadrant anchor inside the cell. Compensating its local
        // offset places all four quarter pivots around the shared cell centre.
        Vector3 quadrantOffset = place.Pizza.localPosition;
        return new Vector3(-quadrantOffset.x + inset, LandingHeight, -quadrantOffset.z + inset);
    }

    private Quaternion GetQuarterRotation(ContainerPlace place)
    {
        int index = GetQuarterIndex(place);
        // The source mesh occupies the back-left quadrant; rotate it into the
        // corresponding slot so four quarters retain a thin '+' seam.
        int rotation = index switch
        {
            1 => 270,
            2 => 90,
            3 => 180,
            _ => 0
        };
        return Quaternion.Euler(0f, rotation, 0f);
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
