import React, { useState } from "react";
import { NavLink } from "react-router-dom";
import { useNavigate } from "react-router-dom";
import '../styles/CreateCourse.css'; // Import the CSS file

function UpdateCourse({authToken, courseId, courseName, setCourseName, description, setDescription, setLoading}){

    document.title = 'Update-Course: Assignment-App'

    const [formData, setFormData] = useState({
        courseName: courseName,
        description: description,
    })

    const [error, setError] = useState(null);
    const navigate = useNavigate();

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData((prevData) => ({ ...prevData, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        setLoading(true);

        try {
            const form = new FormData();
            form.append('courseName', formData.courseName);
            form.append('description', formData.description);
            const response = await fetch(`https://localhost:7110/api/course/${courseId}/UpdateCourse`, {
                method: 'PUT',
                headers: { 
                    'Authorization': `Bearer ${authToken}`,
                },
                body: form,
                credentials: 'include',
            });

            if (response.ok) {
                // Redirect to the courses page or a success page
                setCourseName(formData.courseName)
                setDescription(formData.description)
                navigate('/courseDetails');
            } else {
                const errorData = await response.json();
                setError(errorData.message || 'Failed to create the course.');
            }
        } catch (error) {
            setError('Error creating the course.');
        } finally {
            setLoading(false);
        }
    };

    return(
        <div className="create-course-container" style={{marginTop: 20}}>
            <h3 className="create-course-heading">Add New Course</h3>
            {error && <div className="alert alert-danger">{error}</div>}

            <form onSubmit={handleSubmit} className="create-course-form">
                <input 
                    type="text" 
                    name="courseName" 
                    placeholder="Course Name" 
                    value={formData.courseName} 
                    onChange={handleChange} 
                    className="input-field"
                    required
                />
                <input 
                    type="text" 
                    name="description" 
                    placeholder="Description" 
                    value={formData.description} 
                    onChange={handleChange} 
                    className="input-field"
                    required
                />
                <button type="submit" className="submit-btn">Update</button>
            </form>
            <NavLink to="/courseDetails" className="return-link">Return</NavLink>
        </div>
    );

}

export default UpdateCourse;