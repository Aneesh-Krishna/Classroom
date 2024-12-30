import { useEffect, useState } from "react";
import Spinner from './Loading';
import { NavLink } from "react-router-dom";
import { jwtDecode } from "jwt-decode";

function SubmitAssignment({ authToken, adminId, assignmentId, assignmentText }) {
    document.title = 'Submissions: ClassroomApp';

    const [submissions, setSubmissions] = useState([]);
    const [loading, setLoading] = useState(false);
    const [isAdmin, setIsAdmin] = useState(false);
    const [file, setFile] = useState(null);
    const [submissionText, setSubmissionText] = useState('');

    const disabledRemoveButton = async (authToken) => {
        const decodedToken = jwtDecode(authToken);
        if (adminId === decodedToken.sub) {
            setIsAdmin(true);
        }
    };

    const fetchAllSubmissions = async () => {
        try {
            setLoading(true);
            const response = await fetch(`https://localhost:7110/api/assignmentsubmission/${assignmentId}/GetAllAssignment-Submissions`, {
                method: "GET",
                headers: {
                    Authorization: `Bearer ${authToken}`,
                }
            });

            if (response.ok) {
                const data = await response.json();
                setSubmissions(data?.$values || []);
            } else {
                console.error("An error occurred while fetching the submissions: ", response.statusText);
            }
        } catch (error) {
            console.error("Something went wrong...", error);
        } finally {
            setLoading(false);
        }
    };

    const handleUploadSubmission = async () => {
        try {
            setLoading(true);
            const formData = new FormData();
            formData.append('text', submissionText);
            formData.append('file', file);

            const response = await fetch(`https://localhost:7110/api/assignmentsubmission/${assignmentId}/SubmitAssignment`, {
                method: "POST",
                headers: {
                    Authorization: `Bearer ${authToken}`,
                },
                body: formData,
            });

            if (response.ok) {
                const newSubmission = await response.json();
                setSubmissions((prev) => [...prev, newSubmission]);
                setSubmissionText('');
            } else {
                console.error("An error occurred while submitting the assignment: ", response.statusText);
            }
        } catch (error) {
            console.error("Something went wrong...", error);
        } finally {
            setLoading(false);
        }
    };

    const handleDownload = async (submissionFileName) => {
            try {
                const response = await fetch(`https://localhost:7110/api/file/${submissionFileName}`, {
                  method: "GET",
                  headers: {
                    Authorization: `Bearer ${authToken}`,
                  },
                });
            
                if (response.ok) {
                  // Parse the response as a blob
                  console.log("Response okay..")
                  const blob = await response.blob();
            
                  // Create a temporary link element to initiate the download
                  const url = window.URL.createObjectURL(blob);
                  const link = document.createElement("a");
                  link.href = url;
                  link.download = fileName; // Suggest the filename for the downloaded file
                  document.body.appendChild(link);
                  link.click();
            
                  // Clean up after download
                  document.body.removeChild(link);
                  window.URL.revokeObjectURL(url);
            
                  console.log("File downloaded successfully.");
                }
            
                else{
                  console.error("An error occured while downloading...",response.statusText)
                }
                
              } catch (error) {
                console.error("Something went wrong...:", error);
              }
        };

    useEffect(() => {
        fetchAllSubmissions();
        disabledRemoveButton(authToken);
    }, []);

    return (
        <div className="submissions-container container py-4">
            <div className="submissions-header d-flex justify-content-between align-items-center mb-4">
                <h2 className="text">
                    <NavLink to="/assignments" className="return-link text-decoration-none mx-2">◄</NavLink>
                    Assignment Submissions
                </h2>
                <button
                    type="button"
                    className={`btn btn-${isAdmin ? "primary" : "secondary"} shadow`}
                    title={isAdmin ? "" : "Admin only!"}
                    style={{ pointerEvents: isAdmin ? "auto" : "none" }}
                    data-bs-toggle="modal"
                    data-bs-target="#uploadSubmissionModal"
                >
                    Submit
                </button>
            </div>

            <div
                className="modal fade"
                id="uploadSubmissionModal"
                tabIndex="-1"
                aria-labelledby="uploadSubmissionModalLabel"
                aria-hidden="true"
            >
                <div className="modal-dialog">
                    <div className="modal-content">
                        <div className="modal-header bg-primary text-white">
                            <h5 className="modal-title" id="uploadSubmissionModalLabel">
                                Upload Submission
                            </h5>
                            <button
                                type="button"
                                className="btn-close"
                                data-bs-dismiss="modal"
                                aria-label="Close"
                            ></button>
                        </div>
                        <div className="modal-body">
                            <input 
                                type="text" 
                                className="form-control mb-3"
                                onChange={(e) => setSubmissionText(e.target.value)} 
                                value={submissionText} 
                                placeholder="Enter submission text here"
                            />
                            <input 
                                type="file" 
                                className="form-control" 
                                onChange={(e) => setFile(e.target.files[0])} 
                            />
                        </div>
                        <div className="modal-footer">
                            <button
                                type="button"
                                className="btn btn-secondary"
                                data-bs-dismiss="modal"
                            >
                                Cancel
                            </button>
                            <button 
                                type="button" 
                                disabled={!isAdmin} 
                                onClick={handleUploadSubmission} 
                                className="btn btn-primary" 
                                data-bs-dismiss="modal"
                            >
                                Upload
                            </button>
                        </div>
                    </div>
                </div>
            </div>

            {loading && <Spinner />}

            {submissions.length > 0 ? (
                <ul className="submissions-list row">
                    {submissions.map((submission) => (
                        <li key={submission.assignmentSubmissionId} className="col-md-4 mb-4">
                            <div className="card shadow-sm">
                                <div className="card-body">
                                    <h5 className="card-title text-primary">{assignmentText}</h5>
                                    <p className="card-text">
                                        {submission.submissionFileName ? (
                                            <button
                                                className="btn btn-link p-0"
                                                onClick={() => handleDownload(submission.submissionFileName)}
                                            >
                                                {submission.submissionFileName}
                                            </button>
                                        ) : (
                                            <span className="text-muted">No file</span>
                                        )}
                                    </p>
                                    <p className="card-text">
                                        {submission.text} by{" "}
                                        <strong>{submission.submittedBy?.fullName || "someone"}</strong>
                                    </p>
                                </div>
                            </div>
                        </li>
                    ))}
                </ul>
            ) : (
                <p className="text-muted text-center">No submissions yet!</p>
            )}

            <NavLink to="/courseDetails" className="btn btn-outline-primary mt-3">
                Return
            </NavLink>
        </div>
    );
}

export default SubmitAssignment;
