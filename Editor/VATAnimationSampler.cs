using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace VATSystem
{
    public class VATAnimationSampler : System.IDisposable
    {
        private readonly PlayableGraph graph;
        private readonly AnimationClipPlayable clipPlayable;

        public VATAnimationSampler(Animator animator, AnimationClip clip)
        {
            graph = PlayableGraph.Create("VATSampler");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            clipPlayable = AnimationClipPlayable.Create(graph, clip);
            clipPlayable.SetApplyFootIK(false);
            var output = AnimationPlayableOutput.Create(graph, "Output", animator);
            output.SetSourcePlayable(clipPlayable);
        }

        public void Evaluate(float time)
        {
            clipPlayable.SetTime(time);
            graph.Evaluate();
        }

        public void Dispose()
        {
            if (graph.IsValid()) graph.Destroy();
        }
    }
}