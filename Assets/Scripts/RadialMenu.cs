using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RadialMenu : MonoBehaviour
{
    [SerializeField]
    GameObject OptionPrefab;

    [SerializeField]
    float Radius = 300f;

    [SerializeField]
    List<Texture> Icons;

    List<RadialMenuOption> Options;

    // Start is called before the first frame update
    void Start()
    {
        Options = new List<RadialMenuOption>();
    }

    void AddOption(string pLabel, Texture pIcon)
    {
        GameObject option = Instantiate(OptionPrefab, transform);

        RadialMenuOption rmo = option.GetComponent<RadialMenuOption>();
        rmo.SetLabel(pLabel);
        rmo.SetIcon(pIcon);

        Options.Add(rmo);
    }

    public void Open()
    {
        for (int i = 0; i < Icons.Count; i++)
        {
            AddOption("Button" + i.ToString(), Icons[i]);
        }
        Roundify();
    }

    public void Close()
    {
        for (int i = 0; i < Options.Count; i++)
        {
            Destroy(Options[i].gameObject);
        }
        Options.Clear();
    }

    void Roundify() // Make the menu round, who would have guessed?????
    {
        float radiansOfSeparation = (Mathf.PI * 2) / Options.Count;
        for (int i = 0;i < Options.Count; i++)
        {
            float x = Mathf.Sin(radiansOfSeparation * i) * Radius;
            float y = Mathf.Cos(radiansOfSeparation * i) * Radius;

            Options[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(x, y, 0);
        }
    }

}
