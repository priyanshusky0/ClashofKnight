using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parallex : MonoBehaviour
{
	[Range(-1f, 1f)]
	public float scrollspeed = 0.5f;
	private float offset;
	private Material mat;

    private void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        offset += (Time.deltaTime * scrollspeed) / 10f;
        mat.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }
}
