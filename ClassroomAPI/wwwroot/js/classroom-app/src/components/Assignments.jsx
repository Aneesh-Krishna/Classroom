import React, { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import Spinner from './Loading';
import { jwtDecode } from "jwt-decode";
import { deleteAssignment, downloadAssignmentFile, fetchAllAssignments, uploadAssignment } from "../services/AssignmentService";

function Assignments({ authToken, courseId, courseName, adminId, setAssignmentId, setAssignmentText }) {
    document.title = 'Assignments: ClassroomApp'  

    const [formData, setFormData] = useState({
        text: "",
        file: null,
    });

    const [assignmentText, setAssignmentTextLocal] = useState('');
    const [assignments, setAssignments] = useState([]);
    const [isAdmin, setIsAdmin] = useState(false);
    const [file, setFile] = useState(null);
    const [loading, setLoading] = useState(false);
    const [deleteAssignmentName, setDeleteAssignmentName] = useState('');
    const [deleteAssignmentId, setDeleteAssignmentId] = useState(null);

    const disabledRemoveButton = async (authToken) => {
        const decodedToken = jwtDecode(authToken);
        setIsAdmin(adminId === decodedToken.sub);
    };

    const fetchAssignments = async () => {
        try {
            setLoading(true);
            const data = await fetchAllAssignments(courseId, authToken);
            setAssignments(data?.$values || []);
        } catch (error) {
            console.error("Error fetching assignments: ", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchAssignments();
        disabledRemoveButton(authToken);
    }, []);

    const handleUploadAssignment = async () => {
        try {
            setLoading(true);
            const newAssignment = await uploadAssignment(courseId, assignmentText, file, authToken);
            setAssignments((prev) => [...prev, newAssignment]);
        } catch (error) {
            console.error("Error uploading assignment: ", error);
        } finally {
            setLoading(false);
        }
    };

    const handleDownload = async (assignmentFileName) => {
        try {
            await downloadAssignmentFile(assignmentFileName, authToken);
        } catch (error) {
            console.error("Error downloading the assignment: ", error);
        }
    };

    const handleDeleteAssignment = async (assignmentId) => {
        try {
            await deleteAssignment(assignmentId, authToken);
            fetchAssignments();
        } catch (error) {
            console.error("Error while deleting the assignment...", error);
        }
    };

    return (
        <div className="assignments-container container mt-4">
            <div className="assignments-header text-center mb-4">
                <h1 className="mb-3">
                    <NavLink to="/courseDetails" className="btn btn-outline-primary me-2">◄ Back</NavLink>
                    {courseName}: Assignments
                </h1>
                {isAdmin && (
                    <button
                        className="btn btn-primary"
                        data-bs-toggle="modal"
                        data-bs-target="#uploadAssignmentModal"
                    >
                        Upload Assignment
                    </button>
                )}
            </div>

            {loading ? (
                <div className="d-flex justify-content-center align-items-center" style={{ minHeight: '200px' }}>
                    <Spinner />
                </div>
            ) : assignments.length > 0 ? (
                <ul className="row list-unstyled">
                    {assignments.map((assignment) => (
                        <li key={assignment.assignmentId} className="col-md-4 mb-4">
                            <div className="card shadow-sm h-100">
                                <div className="card-body">
                                    <h5 className="card-title text-truncate"><b>{assignment.text}</b></h5>
                                    <p className="card-subtitle mb-2 text-muted">
                                        {assignment.assignmentFileName ? (
                                            <button
                                                className="btn btn-link p-0 text-decoration-none"
                                                onClick={() => handleDownload(assignment.assignmentFileName)}
                                            >
                                                {assignment.assignmentFileName}
                                            </button>
                                        ) : (
                                            <span>No file available</span>
                                        )}
                                    </p>
                                    <p className="card-text">
                                        All students must submit within 10 days.
                                    </p>
                                    <NavLink
                                        to="/submitAssignment"
                                        onClick={() => { setAssignmentText(assignment.text); setAssignmentId(assignment.assignmentId); }}
                                        className="btn btn-outline-success btn-sm me-2"
                                    >
                                        Submit
                                    </NavLink>
                                    {isAdmin && (
                                        <button
                                            className="btn btn-outline-danger btn-sm"
                                            data-bs-toggle="modal"
                                            data-bs-target="#deleteAssignmentModal"
                                            onClick={() => {
                                                setDeleteAssignmentName(assignment.text);
                                                setDeleteAssignmentId(assignment.assignmentId);
                                            }}
                                        >
                                            Delete
                                        </button>
                                    )}
                                </div>
                            </div>
                        </li>
                    ))}
                </ul>
            ) : (
                <p className="text-center">No assignments found!</p>
            )}

            {/* Upload Assignment Modal */}
            <div className="modal fade" id="uploadAssignmentModal" tabIndex="-1" aria-hidden="true">
                <div className="modal-dialog modal-dialog-centered">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h5 className="modal-title">Upload Assignment</h5>
                            <button type="button" className="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div className="modal-body">
                            <input
                                type="text"
                                className="form-control mb-3"
                                placeholder="Enter assignment text"
                                onChange={(e) => setAssignmentTextLocal(e.target.value)}
                                value={assignmentText}
                            />
                            <input
                                type="file"
                                className="form-control"
                                onChange={(e) => setFile(e.target.files[0])}
                            />
                        </div>
                        <div className="modal-footer">
                            <button className="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button
                                className="btn btn-primary"
                                onClick={handleUploadAssignment}
                                data-bs-dismiss="modal"
                            >
                                Upload
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            {/* Delete Assignment Modal */}
            <div className="modal fade" id="deleteAssignmentModal" tabIndex="-1" aria-hidden="true">
                <div className="modal-dialog modal-dialog-centered">
                    <div className="modal-content">
                        <div className="modal-header">
                            <h5 className="modal-title">Delete Assignment</h5>
                            <button type="button" className="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div className="modal-body">
                            Are you sure you want to delete <b>{deleteAssignmentName}</b>?
                        </div>
                        <div className="modal-footer">
                            <button className="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                            <button
                                className="btn btn-danger"
                                onClick={() => handleDeleteAssignment(deleteAssignmentId)}
                                data-bs-dismiss="modal"
                            >
                                Delete
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default Assignments;
