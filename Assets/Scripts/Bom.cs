using System.Collections;
using System.Collections.Generic;

public class Bom
{
    public int i;
    public int p = 0;  /** 工程数               : Process number */
    public List<int> times; /** 製造スピード : Manufacturing spead */
    public List<int> pTom;  /** 割り当て設備 : Process to Machine */
    public Bom(int _i, int _P) {
        i = _i;
        times = new List<int>(_P);
        pTom = new List<int>(_P);
        for (int p = 0; p < _P; ++p) {
            times.Add(0);
            pTom.Add(0);
        }
    }

}
