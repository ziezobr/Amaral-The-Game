using UnityEngine;
namespace PontoCego
{
    public sealed class OfficeAudio:MonoBehaviour
    {
        AudioSource ambience,foley;AudioClip step,shutter,impact,shot,warning;PlayerMotor motor;float nextStep,nextShot,nextImpact;
        void Start()
        {
            motor=FindAnyObjectByType<PlayerMotor>();ambience=gameObject.AddComponent<AudioSource>();ambience.loop=true;ambience.volume=.08f;
            ambience.clip=Tone("office fan",2,0);ambience.Play();foley=gameObject.AddComponent<AudioSource>();foley.volume=.12f;
            shutter=Tone("shutter",.12f,2);
            impact=Tone("impact",.13f,3);shot=Tone("shot",.11f,4);warning=Tone("warning",.24f,5);
        }
        AudioClip Tone(string name,float duration,int type)
        {
            const int rate=22050;var data=new float[Mathf.RoundToInt(rate*duration)];var random=new System.Random(733+type);
            for(int i=0;i<data.Length;i++)
            {
                float t=i/(float)rate;float noise=(float)random.NextDouble()*2-1;
                data[i]=type==0?(Mathf.Sin(t*2*Mathf.PI*60)*.1f+noise*.025f):noise*Mathf.Exp(-t*(type==1?65:35))*.55f;
                if(type==3)data[i]=(Mathf.Sin(t*2*Mathf.PI*115)*.65f+noise*.25f)*Mathf.Exp(-t*32);
                if(type==4)data[i]=(noise*.65f+Mathf.Sin(t*2*Mathf.PI*82)*.3f)*Mathf.Exp(-t*55);
                if(type==5)data[i]=Mathf.Sin(t*2*Mathf.PI*(330+t*450))*.38f*Mathf.Exp(-t*9);
            }
            var clip=AudioClip.Create(name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        void Update()
        {
            if(ambience)ambience.volume=.08f*GamePreferences.Effects;if(foley)foley.volume=.12f*GamePreferences.Effects;
            if(!motor||Time.timeScale==0)return;
            // Locomotion is deliberately silent; camera, impact and combat effects remain independent.
        }
        public void Shutter(){if(foley)foley.PlayOneShot(shutter,.8f);}
        public void Impact(){if(foley&&Time.time>=nextImpact){nextImpact=Time.time+.045f;foley.PlayOneShot(impact,.8f);}}
        public void Gunshot(){if(foley&&Time.time>=nextShot){nextShot=Time.time+.07f;foley.PlayOneShot(shot,.5f);}}
        public void Warning(){if(foley)foley.PlayOneShot(warning,1);}
        void OnDestroy(){if(step)Destroy(step);if(shutter)Destroy(shutter);if(impact)Destroy(impact);if(shot)Destroy(shot);if(warning)Destroy(warning);if(ambience&&ambience.clip)Destroy(ambience.clip);}
    }
}
