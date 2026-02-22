using UnityEngine;

public class Bomb : SliceableObject
{
    [SerializeField] private GameObject bombFX;

    protected override void OnHitWithBlade(Collider2D col)
    {

        Debug.Log("Bomb hit!");
        Instantiate(bombFX, transform.position, Quaternion.identity);
        ScoreManager.Instance.AddScore(-100); // Subtract points for hitting a bomb
        base.OnHitWithBlade(col);
    }
}
