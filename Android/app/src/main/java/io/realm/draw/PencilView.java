/*
 * Copyright 2016 Realm Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package io.realm.draw;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.widget.ImageView;

public class PencilView extends ImageView {
    private boolean selected;
    private float density;

    public PencilView(Context context) {
        super(context);
        init();
    }

    public PencilView(Context context, AttributeSet attrs) {
        super(context, attrs);
        init();
    }

    public PencilView(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init();
    }

    private void init() {
        density = getContext().getResources().getDisplayMetrics().density;
    }

    public void setSelected(boolean selected) {
        this.selected = selected;
    }

    @Override
    protected void onDraw(Canvas canvas) {
        super.onDraw(canvas);
        if (!selected) {
            return;
        }
        final Paint paint = new Paint();
        paint.setColor(0xccffffff);
        final float top = getHeight() - (12 * density);
        final float rad = 3 * density;
        canvas.drawCircle(getWidth() / 2, top, rad, paint);
    }
}
