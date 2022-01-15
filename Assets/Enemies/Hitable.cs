using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Hitable
{
    void onHit(bool reflect);
}
