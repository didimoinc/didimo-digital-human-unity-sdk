using System.Collections.Generic;

namespace Didimo.Speech
{
    public static class Constants
    {
        public const string PHONEME_SILENCE = "sil";


        public const string p_b_m    = "phoneme_p_b_m";
        public const string d_t_n    = "phoneme_d_t_n";
        public const string s_z      = "phoneme_s_z";
        public const string f_v      = "phoneme_f_v";
        public const string k_g_ng   = "phoneme_k_g_ng";
        public const string ay       = "phoneme_ay";
        public const string r        = "phoneme_r";
        public const string ey_eh_uh = "phoneme_ey_eh_uh";
        public const string aa       = "phoneme_aa";
        public const string ae_ax_ah = "phoneme_ae_ax_ah";
        public const string ao       = "phoneme_ao";
        public const string aw       = "phoneme_aw";

        public static IEnumerable<string> ALL_PHONEMES => new[] {p_b_m, d_t_n, s_z, f_v, k_g_ng, ay, r, ey_eh_uh, aa, ae_ax_ah, ao, aw};
    }
}