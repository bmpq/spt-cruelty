using UnityEngine;

public class CrueltyBorder : MonoBehaviour
{
    [SerializeField] private GameObject divineLight;
    [SerializeField] private GameObject fleshAutomaton;
    [SerializeField] private GameObject powerInMisery;
    [SerializeField] private GameObject hopeEradicated;

    public enum BorderType
    {
        None,
        DivineLight,
        FleshAutomaton,
        PowerInMisery,
        HopeEradicated
    }

    public void SetBorder(BorderType type)
    {
        divineLight.SetActive(type == BorderType.DivineLight);
        fleshAutomaton.SetActive(type == BorderType.FleshAutomaton);
        powerInMisery.SetActive(type == BorderType.PowerInMisery);
        hopeEradicated.SetActive(type == BorderType.HopeEradicated);
    }
}
