import React, { useEffect, useState } from "react";
import Spinner from './Loading';
import "../styles/MaterialsPage.css";
import { jwtDecode } from "jwt-decode";

import {
  fetchMaterials,
  uploadMaterial,
  deleteMaterial,
  downloadMaterial,
} from "../services/MaterialService";
import { NavLink } from "react-router-dom";

function MaterialsPage({ courseId, authToken, adminId }) {
  document.title = 'Materials: ClassroomApp'

  const [materials, setMaterials] = useState([]);
  const [file, setFile] = useState(null);
  const [loading, setLoading] = useState(false);
  const [deleteMaterialName, setDeleteMaterialName] = useState('');
  const [deleteMaterialId, setDeleteMaterialId] = useState(null);
  const [isAdmin, setIsAdmin] = useState(false)
  
  const disabledRemoveButton = async (authToken) => {
    const decodedToken = jwtDecode(authToken);
    if(adminId === decodedToken.sub){
        console.log("Yes...")
        setIsAdmin(true)
        console.log(isAdmin)
    }
    else{
        console.log("Not the admin..")
    }
  }

  const handleFetchMaterials = async () => {
    try {
      const data = await fetchMaterials(courseId, authToken);
      setMaterials(data?.$values || []);
    } catch (error) {
      console.error("Error fetching materials:", error);
    }
  };

  const handleUploadMaterial = async () => {
    if (!file) return alert("Please select a file");

    try {
      setLoading(true);
      const newMaterial = await uploadMaterial(courseId, file, authToken);
      setMaterials((prev) => [...prev, newMaterial]);
    } catch (error) {
      console.error("Error uploading material:", error);
    } finally {
      setLoading(false);
      setFile(null)
    }
  };

  const handleDeleteMaterial = async (materialId) => {
    try {
      await deleteMaterial(materialId, authToken);
      handleFetchMaterials();
    } catch (error) {
      console.error("Error deleting material:", error);
    }
  };

  const handleDownload = async (materialName) => {
    try {
      await downloadMaterial(materialName, authToken);
    } catch (error) {
      console.error("Error downloading material:", error);
    }
  };

  useEffect(() => {
    handleFetchMaterials();
    disabledRemoveButton(authToken)
  }, [authToken, adminId]);

  return (
    <div className="materials-container">
      <div className="materials-header">
        <h1><NavLink to="/courseDetails" className="return-link mx-3" >◄</NavLink>Course Materials</h1>
        <button
                type="button"
                title={isAdmin ? "" : "Admin only!"} 
                style={{pointerEvents: isAdmin ? "auto" : "none", color: isAdmin ? "blue" : "gray"}}
                data-bs-toggle="modal"
                data-bs-target="#uploadMaterialModal"
              >
                Upload
        </button>
        <div
        className="modal fade"
        id="uploadMaterialModal"
        tabIndex="-1"
        aria-labelledby="uploadMaterialModalLabel"
        aria-hidden="true"
        >
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h1 className="modal-title fs-5" id="uploadMaterialModalLabel">
                  Upload Material
                </h1>
                <button
                  type="button"
                  className="btn-close"
                  data-bs-dismiss="modal"
                  aria-label="Close"
                ></button>
              </div>
              <div className="modal-body">
                <input type="file" onChange={(e) => setFile(e.target.files[0])} />
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  data-bs-dismiss="modal"
                >
                  Cancel
                </button>
                <button type="button" disabled={!isAdmin} onClick={handleUploadMaterial} className="btn btn-primary" data-bs-dismiss="modal" >Upload</button>
              </div>
            </div>
          </div>
        </div>
      </div>
      {loading ? <Spinner /> : ""}
      {materials.length > 0 ? (
        <ul className="materials-list">
          {materials.map((material) => (
            <li key={material.materialId}>
              <button onClick={() => handleDownload(material.materialName)}>
                {material.materialName}
              </button>
              <button
                type="button"
                onClick={() => {
                  setDeleteMaterialName(material.materialName);
                  setDeleteMaterialId(material.materialId);
                }}
                title={isAdmin ? "" : "Admin only!"} 
                style={{pointerEvents: isAdmin ? "auto" : "none", color: isAdmin ? "#007bff" : "gray"}}
                data-bs-toggle="modal"
                data-bs-target="#deleteMaterialModal"
              >
                Delete
              </button>
            </li>
          ))}
        </ul>
      ) : (
        <p className="no-materials">No materials found!</p>
      )}

      <div
        className="modal fade"
        id="deleteMaterialModal"
        tabIndex="-1"
        aria-labelledby="deleteMaterialModalLabel"
        aria-hidden="true"
      >
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h1 className="modal-title fs-5" id="deleteMaterialModalLabel">
                {deleteMaterialName}
              </h1>
              <button
                type="button"
                className="btn-close"
                data-bs-dismiss="modal"
                aria-label="Close"
              ></button>
            </div>
            <div className="modal-body">
              Are you sure you want to delete the material?
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
                onClick={() => handleDeleteMaterial(deleteMaterialId)}
                className="btn btn-danger"
                data-bs-dismiss="modal"
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      </div>
      <NavLink to="/courseDetails" className="return-link mx-3" >Return</NavLink>
    </div>
  );
}

export default MaterialsPage;
