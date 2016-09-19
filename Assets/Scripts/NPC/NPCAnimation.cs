using UnityEngine;
using System.Collections.Generic;

public class NPCAnimation : Object
{
	public bool enabled = true;
	public int fps = 10;
	public SpriteRenderer spriteRenderer;
	public bool IsPlaying { get { return mActive; } }
	public List<Sprite> frames { get { return mSprites; } }

	private bool mLoop = true;
	private float mDelta = 0f;
	private int mIndex = 0;
	private bool mActive = false;
	private List<Sprite> mSprites = new List<Sprite>();
	private float lastTime;

	public void Update () {
		if (!enabled && mActive) {
			mActive = false;
		}
		if (mActive && mSprites.Count > 1 && Application.isPlaying && fps > 0f)
		{
			float deltaTime = Time.realtimeSinceStartup - lastTime;
			lastTime = Time.realtimeSinceStartup;

			mDelta += deltaTime;
			float rate = 1f / fps;
			
			if (rate < mDelta)
			{
				
				mDelta = (rate > 0f) ? mDelta - rate : 0f;
				if (++mIndex >= mSprites.Count)
				{
					mIndex = 0;
					enabled = mLoop;
				}

				if (enabled)
				{
					spriteRenderer.sprite = mSprites[mIndex];
				}
			}
		}
	}

	public void Reset()
	{
		mIndex = 0;
		mDelta = 0;

		if (spriteRenderer != null && mSprites.Count > 0)
		{
			spriteRenderer.sprite = mSprites[mIndex];
		}
	}

	public void PlayAnimation(List<Sprite> sprites, bool loop)
	{
		mSprites.Clear();
		mSprites.AddRange (sprites);

		enabled = true;
		mLoop = loop;
		mActive = true;
		mIndex = 0;
		mDelta = 0;
		spriteRenderer.sprite = mSprites.Count > 0 ? mSprites[mIndex] : null;
		lastTime = Time.realtimeSinceStartup;
	}
}
