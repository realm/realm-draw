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
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Path;
import android.hardware.Sensor;
import android.hardware.SensorManager;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.MotionEvent;
import android.view.SurfaceHolder;
import android.view.SurfaceView;
import android.view.View;

import java.util.HashMap;
import java.util.Iterator;

import io.realm.ObjectServerError;
import io.realm.Realm;
import io.realm.RealmList;
import io.realm.RealmResults;
import io.realm.SyncConfiguration;
import io.realm.SyncCredentials;
import io.realm.SyncUser;
import io.realm.draw.models.DrawPath;
import io.realm.draw.models.DrawPoint;
import io.realm.draw.sensor.ShakeSensorEventListener;


public class MainActivity extends AppCompatActivity implements SurfaceHolder.Callback, View.OnClickListener {
    private static final String REALM_URL = "realm://" + BuildConfig.OBJECT_SERVER_IP + ":9080/~/Draw";
    private static final String AUTH_URL = "http://" + BuildConfig.OBJECT_SERVER_IP + ":9080/auth";
    private static final String ID = "demo@realm.io";
    private static final String PASSWORD = "password";
    private static final int EDGE_WIDTH = 683;
    private volatile Realm realm;
    private SurfaceView surfaceView;
    private double ratio = -1;
    private double marginLeft;
    private double marginTop;
    private DrawThread drawThread;
    private String currentColor = "Charcoal";
    private DrawPath currentPath;
    private PencilView currentPencil;
    private HashMap<String, Integer> nameToColorMap = new HashMap<>();
    private HashMap<Integer, String> colorIdToName = new HashMap<>();

    private SensorManager sensorManager;
    private Sensor accelerometerSensor;
    private ShakeSensorEventListener shakeSensorEventListener;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        final SyncCredentials syncCredentials = SyncCredentials.usernamePassword(ID, PASSWORD, false);
        SyncUser.loginAsync(syncCredentials, AUTH_URL, new SyncUser.Callback() {
            @Override
            public void onSuccess(SyncUser user) {
                final SyncConfiguration syncConfiguration = new SyncConfiguration.Builder(user, REALM_URL).build();
                Realm.setDefaultConfiguration(syncConfiguration);
                realm = Realm.getDefaultInstance();
            }

            @Override
            public void onError(ObjectServerError error) {
            }
        });

        surfaceView = (SurfaceView) findViewById(R.id.surface_view);
        surfaceView.getHolder().addCallback(MainActivity.this);

        generateColorMap();
        bindButtons();
        initializeShakeSensor();
    }

    @Override
    protected void onResume() {
        super.onResume();
        sensorManager.registerListener(shakeSensorEventListener, accelerometerSensor, SensorManager.SENSOR_DELAY_UI);
    }

    @Override
    protected void onPause() {
        super.onPause();
        sensorManager.unregisterListener(shakeSensorEventListener);
    }

    private void initializeShakeSensor() {

        sensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
        accelerometerSensor = sensorManager.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);
        shakeSensorEventListener = new ShakeSensorEventListener();
        shakeSensorEventListener.setOnShakeListener(new ShakeSensorEventListener.OnShakeListener() {

            @Override
            public void onShake(int count) {
                wipeCanvas();
            }
        });
    }

    private void bindButtons() {
        int[] buttonIds = {
                R.id.charcoal,
                R.id.elephant,
                R.id.dove,
                R.id.ultramarine,
                R.id.indigo,
                R.id.grape_jelly,
                R.id.mulberry,
                R.id.flamingo,
                R.id.sexy_salmon,
                R.id.peach,
                R.id.melon
        };

        for (int id : buttonIds) {
            View view = findViewById(id);
            view.setOnClickListener(this);
        }

        currentPencil = (PencilView) findViewById(R.id.charcoal);
        currentPencil.setSelected(true);
    }

    private void generateColorMap() {
        nameToColorMap.put("Charcoal", 0xff1c283f);
        nameToColorMap.put("Elephant", 0xff9a9ba5);
        nameToColorMap.put("Dove", 0xffebebf2);
        nameToColorMap.put("Ultramarine", 0xff39477f);
        nameToColorMap.put("Indigo", 0xff59569e);
        nameToColorMap.put("GrapeJelly", 0xff9a50a5);
        nameToColorMap.put("Mulberry", 0xffd34ca3);
        nameToColorMap.put("Flamingo", 0xfffe5192);
        nameToColorMap.put("SexySalmon", 0xfff77c88);
        nameToColorMap.put("Peach", 0xfffc9f95);
        nameToColorMap.put("Melon", 0xfffcc397);
        colorIdToName.put(R.id.charcoal, "Charcoal");
        colorIdToName.put(R.id.elephant, "Elephant");
        colorIdToName.put(R.id.dove, "Dove");
        colorIdToName.put(R.id.ultramarine, "Ultramarine");
        colorIdToName.put(R.id.indigo, "Indigo");
        colorIdToName.put(R.id.grape_jelly, "GrapeJelly");
        colorIdToName.put(R.id.mulberry, "Mulberry");
        colorIdToName.put(R.id.flamingo, "Flamingo");
        colorIdToName.put(R.id.sexy_salmon, "SexySalmon");
        colorIdToName.put(R.id.peach, "Peach");
        colorIdToName.put(R.id.melon, "Melon");
    }

    private void wipeCanvas() {
        if(realm != null) {
            realm.executeTransactionAsync(new Realm.Transaction() {
                @Override
                public void execute(Realm r) {
                    r.deleteAll();
                }
            });
        }
    }


    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (realm != null) {
            realm.close();
            realm = null;
        }
    }

    @Override
    public boolean onTouchEvent(MotionEvent event) {

        if(realm == null) {
            return false; // if we are in the middle of a rotation, realm may be null.
        }

        int[] viewLocation = new int[2];
        surfaceView.getLocationInWindow(viewLocation);
        int action = event.getAction();
        if (action == MotionEvent.ACTION_DOWN
                || action == MotionEvent.ACTION_MOVE
                || action == MotionEvent.ACTION_UP
                || action == MotionEvent.ACTION_CANCEL) {
            float x = event.getRawX();
            float y = event.getRawY();
            double pointX = (x - marginLeft - viewLocation[0]) * ratio;
            double pointY = (y - marginTop - viewLocation[1]) * ratio;

            if (action == MotionEvent.ACTION_DOWN) {
                realm.beginTransaction();
                currentPath = realm.createObject(DrawPath.class);
                currentPath.setColor(currentColor);
                DrawPoint point = realm.createObject(DrawPoint.class);
                point.setX(pointX);
                point.setY(pointY);
                currentPath.getPoints().add(point);
                realm.commitTransaction();
            } else if (action == MotionEvent.ACTION_MOVE) {
                realm.beginTransaction();
                DrawPoint point = realm.createObject(DrawPoint.class);
                point.setX(pointX);
                point.setY(pointY);
                currentPath.getPoints().add(point);
                realm.commitTransaction();
            } else if (action == MotionEvent.ACTION_UP) {
                realm.beginTransaction();
                currentPath.setCompleted(true);
                DrawPoint point = realm.createObject(DrawPoint.class);
                point.setX(pointX);
                point.setY(pointY);
                currentPath.getPoints().add(point);
                realm.commitTransaction();
                currentPath = null;
            } else {
                realm.beginTransaction();
                currentPath.setCompleted(true);
                realm.commitTransaction();
                currentPath = null;
            }
            return true;

        }
        return false;
    }

    @Override
    public void surfaceCreated(SurfaceHolder surfaceHolder) {
        if (drawThread == null) {
            drawThread = new DrawThread();
            drawThread.start();
        }
    }

    @Override
    public void surfaceChanged(SurfaceHolder surfaceHolder, int format, int width, int height) {
        boolean isPortrait = width < height;
        if (isPortrait) {
            ratio = (double) EDGE_WIDTH / height;
        } else {
            ratio = (double) EDGE_WIDTH / width;
        }
        if (isPortrait) {
            marginLeft = (width - height) / 2.0;
            marginTop = 0;
        } else {
            marginLeft = 0;
            marginTop = (height - width) / 2.0;
        }
    }

    @Override
    public void surfaceDestroyed(SurfaceHolder surfaceHolder) {
        if (drawThread != null) {
            drawThread.shutdown();
            drawThread = null;
        }
        ratio = -1;
    }

    @Override
    public void onClick(View view) {
        String colorName = colorIdToName.get(view.getId());
        if (colorName == null) {
            return;
        }
        currentColor = colorName;
        if (view instanceof PencilView) {
            currentPencil.setSelected(false);
            currentPencil.invalidate();
            PencilView pencil = (PencilView) view;
            pencil.setSelected(true);
            pencil.invalidate();
            currentPencil = pencil;
        }
    }

    class DrawThread extends Thread {
        private Realm bgRealm;

        public void shutdown() {
            synchronized(this) {
                if (bgRealm != null) {
                    bgRealm.stopWaitForChange();
                }
            }
            interrupt();
        }

        @Override
        public void run() {
            while (ratio < 0 && !isInterrupted()) {
            }

            if (isInterrupted()) {
                return;
            }

            Canvas canvas = null;

            try {
                final SurfaceHolder holder = surfaceView.getHolder();
                canvas = holder.lockCanvas();
                canvas.drawColor(Color.WHITE);
            } finally {
                if (canvas != null) {
                    surfaceView.getHolder().unlockCanvasAndPost(canvas);
                }
            }

            while (realm == null && !isInterrupted()) {
            }

            if (isInterrupted()) {
                return;
            }

            bgRealm = Realm.getDefaultInstance();
            final RealmResults<DrawPath> results = bgRealm.where(DrawPath.class).findAll();

            while (!isInterrupted()) {
                try {
                    final SurfaceHolder holder = surfaceView.getHolder();
                    canvas = holder.lockCanvas();

                    synchronized (holder) {
                        canvas.drawColor(Color.WHITE);
                        final Paint paint = new Paint();
                        for (DrawPath drawPath : results) {
                            final RealmList<DrawPoint> points = drawPath.getPoints();
                            final Integer color = nameToColorMap.get(drawPath.getColor());
                            if (color != null) {
                                paint.setColor(color);
                            } else {
                                paint.setColor(nameToColorMap.get(currentColor));
                            }
                            paint.setStyle(Paint.Style.STROKE);
                            paint.setStrokeWidth((float) (4 / ratio));
                            final Iterator<DrawPoint> iterator = points.iterator();
                            final DrawPoint firstPoint = iterator.next();
                            final Path path = new Path();
                            final float firstX = (float) ((firstPoint.getX() / ratio) + marginLeft);
                            final float firstY = (float) ((firstPoint.getY() / ratio) + marginTop);
                            path.moveTo(firstX, firstY);
                            while(iterator.hasNext()) {
                                DrawPoint point = iterator.next();
                                final float x = (float) ((point.getX() / ratio) + marginLeft);
                                final float y = (float) ((point.getY() / ratio) + marginTop);
                                path.lineTo(x, y);
                            }
                            canvas.drawPath(path, paint);
                        }
                    }
                } finally {
                    if (canvas != null) {
                        surfaceView.getHolder().unlockCanvasAndPost(canvas);
                    }
                }
                bgRealm.waitForChange();
            }

            synchronized(this) {
                bgRealm.close();
            }
        }
    }
}
