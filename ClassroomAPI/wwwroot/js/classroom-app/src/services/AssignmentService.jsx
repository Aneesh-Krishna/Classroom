
const BASE_URL = "https://localhost:7110/api/assignment";

export const fetchAllAssignments = async (courseId, authToken) => {

    try{
        const response = await fetch(`https://localhost:7110/api/assignment/${courseId}/assignments`, {
            method: "GET",
            headers: {
                Authorization: `Bearer ${authToken}`,
            }
        });

        if(response.ok){
            return response.json()
        }
        else{
            console.error("An error occured while fetching the assignments...", response.statusText)
        }
    }
    catch(error){
        console.error("Something went wrong...", error)
    }
}

export const uploadAssignment = async (courseId, text, file, authToken) => {
    try{
        const formData = new FormData()
        formData.append('text', text)
        formData.append('file',file)

        const response = await fetch(`${BASE_URL}/${courseId}/Post-Assignment`, {
            method: "POST",
            headers: {
                Authorization: `Bearer ${authToken}`,
            },
            body: formData,
        });

        if(response.ok){
            return response.json()
        }
        else{
            console.error("An error occured while uploading the assignment: ", response.statusText)
        }
    }
    catch(error){
        console.error("Something went wrong...", error)
    }
}

export const downloadAssignmentFile = async (fileName, authToken) => {
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
}

export const deleteAssignment = async (assignmentId, authToken) => {
    try{
        const response = await fetch(`${BASE_URL}/${assignmentId}/DeleteAssignment`, {
            method: "DELETE",
            headers: {
                Authorization: `Bearer ${authToken}`,
            },
        });

        if(response.ok){
            console.log("Assignment deleted successfully!");
            return;
        }
        else{
            console.error("An error occured while deleting the assignment: ", response.statusText)
        }
    }
    catch(error){
        console.error("Something went wrong...", error)
    }
}