using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CurvedPathGenerator
{
    [RequireComponent(typeof(Rigidbody))]
    public class PathFollower : MonoBehaviour
    {
        public UnityEngine.Events.UnityEvent EndEvent;
        public PathGenerator Generator;
        public float Speed = 100f;
        public float DistanceThreshold = 0.2f;
        public float TurningSpeed = 10f;
        public bool IsLoop = false;
        public bool IsMove = true;
        public bool IsEndEventEnable = false;
        public bool EnableCheckFlag = true;
        private bool checkFlag = false;
        private Rigidbody targetRigidbody;
        private GameObject target => gameObject;
        private Vector3 nextPath;
        private int pathIndex = 1;
        public float PassedDistancePercent
        {
            get
            {
                if (Generator == null) return 0f;
                return GetPassedLength() / Generator.GetLength() * 100f;
            }
        }
        private bool AlreadyPositionSet = false;

        private void Start()
        {
            targetRigidbody = GetComponent<Rigidbody>();

            if (Generator != null && !AlreadyPositionSet)
            {
                nextPath = Generator.PathList[1];
                transform.position = Generator.PathList[0];
            }
        }
        public void FixedUpdate()
        {
            if (!IsMove)
            {
                targetRigidbody.velocity = Vector3.zero;
                return;
            }

            if (Generator == null)
            {
                IsMove = false;

                if (EnableCheckFlag)
                    checkFlag = false;

                return;
            }

            if (!checkFlag && EnableCheckFlag)
            {
                checkFlag = true;

                nextPath = Generator.PathList[1];
                transform.position = Generator.PathList[0];
            }

            Vector3 offset = nextPath - target.transform.position;
            offset.Normalize();
            Quaternion q = Quaternion.LookRotation(offset);
            targetRigidbody.rotation = Quaternion.Slerp(targetRigidbody.rotation, q, TurningSpeed * Time.deltaTime);

            offset.Normalize();
            targetRigidbody.velocity = Speed * Time.deltaTime * offset;

            float Distance = Vector3.Distance(nextPath, target.transform.position);

            if (Distance < DistanceThreshold)
            {
                if (pathIndex + 1 < Generator.PathList.Count)
                {
                    nextPath = Generator.PathList[++pathIndex];
                }
                else
                {
                    if (Generator.IsClosed)
                    {
                        if (IsLoop)
                        {
                            if (EndEvent != null && IsEndEventEnable)
                            {
                                EndEvent.Invoke();
                            }
                            nextPath = Generator.PathList[0];
                            pathIndex = 0;
                        }
                        else
                        {
                            StopFollow();
                            if (EndEvent != null && IsEndEventEnable)
                            {
                                EndEvent.Invoke();
                            }
                        }
                    }
                    else
                    {
                        if (IsLoop)
                        {
                            nextPath = Generator.PathList[1];
                            pathIndex = 1;
                            this.transform.position = Generator.PathList[0];
                            target.transform.LookAt(Generator.PathList[1]);

                            if (EndEvent != null && IsEndEventEnable)
                            {
                                EndEvent.Invoke();
                            }
                        }
                        else
                        {
                            StopFollow();
                            if (EndEvent != null && IsEndEventEnable)
                            {
                                EndEvent.Invoke();
                            }
                        }
                    }
                }
            }
        }

        public float GetPassedLength()
        {
            if (Generator == null) return -1;

            if (pathIndex <= 1)
            {
                return (Generator.PathList[0] - this.transform.position).magnitude;
            }
            else if (pathIndex >= Generator.PathList.Count)
            {
                return Generator.GetLength();
            }
            else
            {
                return Generator.PathLengths[pathIndex - 2] + (Generator.PathList[pathIndex - 1] - this.transform.position).magnitude;
            }
        }
        public void StopFollow()
        {
            IsMove = false;
        }
        public void StartFollow()
        {
            if (Generator == null)
            {
                return;
            }
            IsMove = true;
        }
        public void InsertIntoPath(Vector3 position)
        {
            StartFollow();
            transform.position = position;

            pathIndex = Generator.GetNearestPathPointIndex(position);

            if (pathIndex + 1 < Generator.PathList.Count)
                nextPath = Generator.PathList[pathIndex + 1];

            AlreadyPositionSet = true;
        }

        public void ResetProgress()
        {
            pathIndex = 1;
            nextPath = Generator.PathList[1];
            transform.position = Generator.PathList[0];
            AlreadyPositionSet = false;
        }
    }
}
