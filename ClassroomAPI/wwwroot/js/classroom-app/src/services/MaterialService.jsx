// src/services/MaterialService.js

const BASE_URL = "https://localhost:7110/api/material"; // Replace with your API URL

// Fetch all materials for a course
export const fetchMaterials = async (courseId, authToken) => {
  
  try{
    const response = await fetch(`https://localhost:7110/api/material/${courseId}/GetMaterials`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${authToken}`,
      },
    });
  
    if(response.ok){
      return response.json()
    }
    else{
      console.error("Failed to fetch materials:", response.statusText);
    }
  }
  catch(error){
    console.error("Something went wrong...",error)
  }
};

// Fetch material by ID
export const fetchMaterialById = async (materialId, authToken) => {
  
  try{
    const response = await fetch(`${BASE_URL}/${materialId}/GetMaterial`, {
      method: "GET",
      headers: {
        Authorization: `Bearer ${authToken}`,
      },
    });
  
    if(response.ok){
      return response.json();
    }
    else{
      console.error("Error fetching the material: ",response.statusText)
    }
  }
  catch(error){
    console.error("Something went wrong...",error);
  }
};

// Upload a new material
export const uploadMaterial = async (courseId, file, authToken) => {
  try{
    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch(`${BASE_URL}/${courseId}/UploadMaterial`, {
      method: "POST",
      headers: {
        Authorization: `Bearer ${authToken}`,
      },
      body: formData,
    });

    if(response.ok){
      return response.json();
    }
    else{
      console.error("An error occured while uploading: ",response.statusText)
    }
  }
  catch(error){
    console.error("Something went wrong...", error)
  }
};

// Delete a material
export const deleteMaterial = async (materialId, authToken) => {
  try{
    const response = await fetch(`${BASE_URL}/${materialId}/DeleteMaterial`, {
      method: "DELETE",
      headers: {
        Authorization: `Bearer ${authToken}`,
      },
    });
  
    if (response.ok) {
      return;
    }
    else{
      console.error("An error occured while deleting the material: ",response.statusText)
    }
  }
  catch(error){
    console.error("Something went wrong...",error)
  }
};

export const downloadMaterial = async (fileName, authToken) => {
  try {
    const response = await fetch(`https://localhost:7110/api/file/${fileName}`, {
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
