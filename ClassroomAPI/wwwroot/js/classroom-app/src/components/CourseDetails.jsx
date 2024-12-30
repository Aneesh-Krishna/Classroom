import { useEffect, useState } from "react";
import "../styles/CourseDetails.css"; // Include a separate CSS file for styles
import { NavLink, useNavigate } from "react-router-dom";
import Spinner from './Loading';
import { jwtDecode } from "jwt-decode";

function CourseDetails({ authToken, courseId, courseName, admin, adminId, description }) {
    document.title = 'Course-details: Assignment-App';

    const navigate = useNavigate();

    const [loading, setLoading] = useState(false);
    const [members, setMembers] = useState([]);
    const [searchUserName, setSearchUserName] = useState('');
    const [searchUserNameResults, setSearchUsernameResults] = useState([])
    const [selectedUserId, setSelectedUserId] = useState('')
    const [isAdmin, setIsAdmin] = useState(false)

    const disabledRemoveButton = async (authToken) => {
        const decodedToken = jwtDecode(authToken);
        if(adminId === decodedToken.sub){
            setIsAdmin(true)
        }
        else{
            console.log("Not the admin..")
        }
    }

    const fetchCourses = async () => {
        try {
            setLoading(true); // Set loading to true before fetching data
            const response = await fetch(`https://localhost:7110/api/course/${courseId}/members`, {
                method: "GET",
                headers: { Authorization: `Bearer ${authToken}` },
            });

            if (response.ok) {
                const data = await response.json(); // Parse the response JSON
                // console.log("Fetched courses:", data);
                setMembers(data?.$values || []);
            } else {
                console.error("Failed to fetch courses:", response.statusText);
            }
        } catch (error) {
            console.error("Error fetching courses:", error);
        } finally {
            setLoading(false); // Set loading to false after fetching data
        }
    };

    useEffect(() => {
        fetchCourses();
        disabledRemoveButton(authToken);
    }, [authToken, adminId]);

    const handleDelete = async (e) =>{
        e.preventDefault()

        try{
            const response = await fetch(`https://localhost:7110/api/course/${courseId}`, {
                method: "DELETE",
                headers: { 
                    'Authorization': `Bearer ${authToken}`,
                },
            })

            if(response.ok){
                console.log("Course deleted!")
                navigate('/courses')
            }
            else{
                console.error("Failed to delete the course!"+ response.statusText)
            }
        }
        catch(error){
            console.error("Something went wrong... ", error)
        }
    }

    const handleSearchUser = async (e) => {
        e.preventDefault()
        if (!searchUserName.trim()) return;

        try{
                const response = await fetch(`https://localhost:7110/api/account/${searchUserName}/users`, {
                method: "GET",
                headers: { Authorization: `Bearer ${authToken}` },
            });

            if(response.ok){
                const data = await response.json()
                setSearchUsernameResults(data?.$values || [])
            }
            else{
                const errorData = await response.json()
                console.error(errorData)
            }
            
        }
        catch(error){
            console.error("Something went wrong...", error);
        }
    }

    const handleAddUser = async (e) => {
        e.preventDefault()

        try{
            const form = new FormData()
            form.append('newUserId', selectedUserId)

            const response = await fetch(`https://localhost:7110/api/course/${courseId}/AddMember`, {
                method: "POST",
                headers: {
                    Authorization: `Bearer ${authToken}`,
                },
                body: form,
            });

            if(response.ok){
                setSelectedUserId('')
                fetchCourses()
            }
            else{
                const errorData = await response.json()
                console.error(errorData)
            }
        }
        catch(error){
            console.error("Something went wrong...", error)
        }
    }

    const handleRemoveUser = async (e) => {
        e.preventDefault()

        try{
            const response = await fetch(`https://localhost:7110/api/course/${selectedUserId}/RemoveMember`, {
                method: "POST",
                headers: {
                    Authorization: `Bearer ${authToken}`,
                },
            });

            if(response.ok){
                setSelectedUserId('')
                fetchCourses()
            }
            else{
                const errorData = await response.json()
                console.error(errorData)
            }
        }
        catch(error){
            console.error("Something went wrong...",error);
        }
    }

    return (
        <div className="course-details-container" style={{marginTop: 50}}>
            <div className="course-header">
                <h1 className="course-title">{courseName}</h1>
                <p className="course-admin">Admin: <strong>{admin}</strong></p>
                <p className="course-description">{description}</p>
            </div>

            <div className="course-members">
                <h2>Members 
                    <button disabled={!isAdmin} onClick={() => (setSearchUserName(''), setSearchUsernameResults(''))} data-bs-toggle="modal" data-bs-target="#staticBackdrop">➕</button>
                </h2>
                {loading ? (
                    <Spinner/>
                ) : members.length > 0 ? (
                    <ul className="member-list">
                        {members.map((member) => (
                            <li key={member.id} className="member-item">
                                <span className="member-icon">👤</span>
                                {member.name}
                                <span className="remove-icon"  type="button" title={isAdmin ? "" : "Admin only!"} style={{pointerEvents: isAdmin ? "auto" : "none", color: isAdmin ? "#007bff" : "gray"}} data-bs-toggle="modal" data-bs-target="#staticBackdropRemove" onClick={() => (setSelectedUserId(member.memberId))}>❌</span>
                            </li>
                        ))}
                    </ul>
                ) : (
                    <p className="no-members-text">No Members!</p>
                )}
            </div>

            <div className="modal fade" id="staticBackdropRemove" aria-labelledby="exampleModalLabel" aria-hidden="true">
            <div className="modal-dialog">
                <div className="modal-content">
                    <div className="modal-body">
                        Are you sure you want to remove the user from {courseName}?
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="button" disabled={!isAdmin} onClick={handleRemoveUser} className="btn btn-danger" data-bs-dismiss="modal">Remove</button>
                    </div>
                    </div>
                </div>
            </div>

            <div className="modal fade" id="staticBackdrop" data-bs-backdrop="static" data-bs-keyboard="false" aria-labelledby="staticBackdropLabel" aria-hidden="true">
                <div className="modal-dialog">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h1 className="modal-title fs-5" id="staticBackdropLabel">
                                Add Members
                                <form className="d-flex" role="search" onSubmit={handleSearchUser}>
                                    <input className="form-control me-2" onChange={(e) => (setSearchUserName(e.target.value), setSearchUsernameResults(''))} type="search" placeholder="Search for users" value={searchUserName} aria-label="Search" />
                                    <button className="btn btn-outline-success" type="submit">Search</button>
                                </form>
                            </h1>
                        </div>
                        <div className="modal-body">
                                {searchUserNameResults.length > 0 ? 
                                    (<ul className="list-group">
                                        {searchUserNameResults.map((user) => (
                                            <li 
                                                key={user.id}
                                                className={`list-group-item`}
                                                onClick={() => setSelectedUserId(user.id)}
                                                style={{ cursor: 'pointer' }}
                                            >
                                                <div className="d-flex justify-content-between align-items-center">
                                                    {user.userName}
                                                </div>
                                            </li>
                                        ))}
                                    </ul>) : (
                                                <p>No such user found!</p>
                                            )
                                }
                        </div>
                        <div className="modal-footer">
                            <button type="button" onClick={() => (setSearchUserName(''), setSearchUsernameResults(''))} className="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" disabled={!isAdmin} onClick={handleAddUser} className="btn btn-primary" data-bs-dismiss="modal">Add</button>
                        </div>
                    </div>
                </div>
            </div>

            <div className="course-actions">
            <NavLink to="/updateCourse" className="return-link mx-3" title={isAdmin ? "" : "Admin only!"} style={{pointerEvents: isAdmin ? "auto" : "none", color: isAdmin ? "#007bff" : "gray"}}>Edit</NavLink>
            <a type="button" disabled={!isAdmin} style={{pointerEvents: isAdmin ? "auto" : "none", color: isAdmin ? "#007bff" : "gray"}} className="btn return-link" data-bs-toggle="modal" data-bs-target="#exampleModal">
                Delete
            </a>
            <div className="modal fade" id="exampleModal"  aria-labelledby="exampleModalLabel" aria-hidden="true">
            <div className="modal-dialog">
                <div className="modal-content">
                    <div className="modal-body">
                        Are you sure you want to delete {courseName}?
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="button" disabled={!isAdmin} onClick={handleDelete} className="btn btn-danger" data-bs-dismiss="modal">Delete </button>
                    </div>
                    </div>
                </div>
            </div>
            {/* <NavLink to="/deleteCourse" className="return-link">Delete</NavLink> */}
            <NavLink to="/materials" className="return-link mx-3">Materials</NavLink>
            <NavLink to="/assignments" className="return-link mx-3">Assignments</NavLink>
            <NavLink to="/courses" className="return-link mx-3">Back to Courses</NavLink>
            </div>

        </div>
    );
}

export default CourseDetails;
