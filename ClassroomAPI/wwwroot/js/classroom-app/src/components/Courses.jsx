import React, { useEffect, useState } from 'react';
import Spinner from './Loading';
import { NavLink } from 'react-router-dom';
import { useNavigate } from 'react-router-dom';
import '../styles/Courses.css'; // Add a CSS file for styling.

function Courses({ authToken, setCourseId, setLoading, setCourseName, setAdmin, setAdminId, setDescription, setAuthToken }) {
    document.title = 'Courses: Assignment-App';
    const [courses, setCourses] = useState([]);
    const navigate = useNavigate();

    const fetchCourses = async () => {
        try {
            setLoading(true); // Set loading to true before fetching data
            const response = await fetch('https://localhost:7110/api/course', {
                method: 'GET',
                headers: { Authorization: `Bearer ${authToken}` },
            });

            if (response.ok) {
                const data = await response.json(); // Parse the response JSON
                setCourses(data?.$values || []);
            } else {
                console.error('Failed to fetch courses:', response.statusText);
            }
        } catch (error) {
            console.error('Error fetching courses:', error);
        } finally {
            setLoading(false); // Set loading to false after fetching data
        }
    };

    useEffect(() => {
        fetchCourses();
    }, [authToken]);

    return (
        <div className="courses-container" style={{marginTop: 50}}>
            <h3 className="text-center">Your Courses</h3>
            <NavLink to="/createCourse"
                className="create-course-link"
                onClick={() => setAuthToken(authToken)}
            >
                Create new course 
            </NavLink>
            {courses.length <= 0 ? <p><b>You've not enrolled in any courses!</b></p> : <p></p>}
            <div className="courses-grid">
                {courses.map((course) => (
                    <div className="course-card" key={course.courseId}>
                        <div className="course-card-header">
                            <h5>{course.courseName}</h5>
                            <p className="course-admin">Instructor: {course.admin}</p>
                        </div>
                        <div className="course-card-body">
                            <p>{course.description}</p>
                        </div>
                        <div className="course-card-footer">
                            <NavLink
                                to="/courseChats"
                                className="details-link"
                                onClick={() => {
                                    setCourseId(course.courseId);
                                    setCourseName(course.courseName);
                                    setAdmin(course.admin);
                                    setAdminId(course.adminId);
                                    setDescription(course.description);
                                    setAuthToken(authToken);
                                }}
                            >
                                Chats
                            </NavLink>
                        </div>
                        <div className="course-card-footer">
                            <NavLink
                                to="/courseDetails"
                                className="details-link"
                                onClick={() => {
                                    setCourseId(course.courseId);
                                    setCourseName(course.courseName);
                                    setAdmin(course.admin);
                                    setAdminId(course.adminId);
                                    setDescription(course.description);
                                    setAuthToken(authToken);
                                }}
                            >
                                View Details
                            </NavLink>
                        </div>
                    </div>
                ))}
            </div>
        </div>
    );
}

export default Courses;
