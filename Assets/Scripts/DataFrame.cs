using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataFrame 
{
    public const int DEFAULT_CAPACITY = 8;
    public const int SECONDS_A_DAY = 86400;
    public const int MAX_DAY = 1000;
    public int M, I, P, R, BL;
    public List<int> C;
    public List<int> D;
    public List<Order> orders;
    public List<Operation> operations;
    public List<Bom> boms;
    public List<List<int> > mdToC;
    public int AL, OL;

    public int MAX_D = 0;
    public int MAX_C = 8;

    public long gross_profit, cost_sum, total_profit;
    public List<int> deadLineViolationOrders;

    void readProblem() {
        /* input.txt　の読み込み */
        StreamReader sr = new StreamReader(Application.dataPath + "/input.txt");
        string _s = sr.ReadLine();
        string[] input = _s.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
        M = int.Parse(input[1]);
        I = int.Parse(input[2]);
        P = int.Parse(input[3]);
        R = int.Parse(input[4]);
        BL = int.Parse(input[5]);

        _s = sr.ReadLine();
        input = _s.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
        C = new List<int>(M);
        for (int m = 0; m < M; ++m) {
            C.Add(int.Parse(input[m+1]));
        }

        _s = sr.ReadLine();
        input = _s.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
        D = new List<int>(M);
        for (int m = 0; m < M; ++m) {
            D.Add(int.Parse(input[m+1]));
        }

        boms = new List<Bom>(I);
        for (int i = 0; i < I; ++i) boms.Add(new Bom(i, P));
        for (int n = 0; n < BL; ++n) {
            int i, p, m, s;
            _s = sr.ReadLine();
            input = _s.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            i = int.Parse(input[1]);
            p = int.Parse(input[2]);
            m = int.Parse(input[3]);
            s = int.Parse(input[4]);
            --i; --p; --m;
            boms[i].p = Math.Max(boms[i].p, p+1);
            boms[i].pTom[p] = m;
            boms[i].times[p] = s;
        }

        orders = new List<Order>(R);
        for (int r = 0; r < R; ++r) orders.Add(new Order());
        for (int n = 0; n < R; ++n) {
            int r, i, e, d, q, pr;
            _s = sr.ReadLine();
            input = _s.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            r = int.Parse(input[1]);
            i = int.Parse(input[2]);
            e = int.Parse(input[3]);
            d = int.Parse(input[4]);
            q = int.Parse(input[5]);
            pr = int.Parse(input[6]);
            --r; --i;
            orders[r].r = r;
            orders[r].i = i;
            orders[r].e = e;
            orders[r].d = d;
            orders[r].q = q;
            orders[r].pr = pr;
        }
        sr.Close();

        /* output.txt の読み込み */
        sr = new StreamReader(Application.dataPath + "/output.txt");
        _s = sr.ReadLine();
        string[] output = _s.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
        AL = int.Parse(output[0]);

        mdToC = new List<List<int>>(M);
        for (int m = 0; m < M; ++m) {
            mdToC.Add(new List<int>(MAX_DAY));
            for (int d = 0; d <= MAX_DAY; ++d) {
                mdToC[m].Add(DEFAULT_CAPACITY);
            }
        }

        for (int n = 0; n < AL; ++n) {
            int m, d, ac;
            _s = sr.ReadLine();
            output = _s.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            m = int.Parse(output[0]);
            d = int.Parse(output[1]);
            ac = int.Parse(output[2]);
            --m;
            mdToC[m][d] += ac;
            MAX_C = Math.Max(mdToC[m][d], MAX_C);
            MAX_D = Math.Max(d, MAX_D);
        }


        _s = sr.ReadLine();
        output = _s.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
        OL = int.Parse(output[0]);

        operations = new List<Operation>(R);
        for (int r = 0; r < R; ++r) operations.Add(new Operation());
        for (int n = 0; n < OL; ++n) {
            int r, p, m, t1, t2;
            _s = sr.ReadLine();
            output = _s.Split((string[])null, StringSplitOptions.RemoveEmptyEntries);
            r = int.Parse(output[0]);
            p = int.Parse(output[1]);
            m = int.Parse(output[2]);
            t1 = int.Parse(output[3]);
            t2 = int.Parse(output[4]);
            --r; --p; --m;
            if (operations[r].bom == null) {
                operations[r].bom = boms[orders[r].i];
                operations[r].t1 = new List<int>(operations[r].bom.p);
                operations[r].t2 = new List<int>(operations[r].bom.p);
                operations[r].pTom = new List<int>(operations[r].bom.p);
                operations[r].mTop = new List<int>(M);
                for (int _m = 0; _m < M; ++_m) {
                    operations[r].mTop.Add(-1);
                }
                for (int _p = 0; _p < operations[r].bom.p; ++_p) {
                    operations[r].t1.Add(0);
                    operations[r].t2.Add(0);
                    operations[r].pTom.Add(0);
                }
            }
            operations[r].r = r;
            operations[r].pTom[p] = m;
            operations[r].mTop[m] = p;
            operations[r].t1[p] = t1;
            operations[r].t2[p] = t2;
        }

        for (int i = 0; i < operations.Count; ++i) {
            int t2 = operations[i].t2[operations[i].t2.Count - 1];
            int d = (t2 + SECONDS_A_DAY - 1)/SECONDS_A_DAY;
            MAX_D = Math.Max(d, MAX_D);
        }
    }

    void evaluate() {
        deadLineViolationOrders = new List<int>();
        for (int r = 0; r < R; ++r) {
            int d = (orders[r].d - 1) * SECONDS_A_DAY;
            int t2 = operations[r].t2[operations[r].t2.Count - 1];
            if (t2 <= d) gross_profit += orders[r].pr; 
            else {
                deadLineViolationOrders.Add(r);
            }
        }

        double tmp_cost = 0;
        for (int m = 0; m < M; ++m) {
            for (int d = 1; d <= MAX_D; ++d) {
                if (mdToC[m][d] != SECONDS_A_DAY) {
                    tmp_cost += (double)C[m] * Mathf.Pow(mdToC[m][d]-DEFAULT_CAPACITY, 1.1f);
                }
            }
        }
        cost_sum = (long)Math.Ceiling(tmp_cost);

        total_profit = gross_profit - cost_sum;
    }

    public void init() {
        readProblem();
        evaluate();
    }

}