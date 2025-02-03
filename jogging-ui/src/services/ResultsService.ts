import { FetchResultsParams, Result } from '@/types';
import axiosInstance from '../api/axiosConfig';  // Import the axiosInstance

/**
 * Fetch all results for a given competition from the 'all-results' API endpoint
 * @param params - Parameters to control pagination and sorting of results
 * @returns A list of results
 */
export const fetchAllResults = async (
  params: FetchResultsParams = { pageNumber: 1, pageSize: 10, orderBy: 'a' }
): Promise<Result[] | null> => {
  try {
    // Fetch results from the 'all-results' API endpoint
    const response = await axiosInstance.get('result/all-results', {
      params: {
        PageNumber: params.pageNumber,
        PageSize: params.pageSize,
        OrderBy: params.orderBy,
      },
    });

    const data = response.data; // The API response containing all results

    return data;
  } catch (error: any) {
    console.error(
      'Failed to fetch all results:',
      error?.response?.data || error.message
    );
    return null;
  }
};

/**
 * Fetch results for a specific competition
 * @param params - Parameters with the competitionId for fetching results
 * @returns A list of results related to the specific competition
 */

// This function will fetch results and format them for use in your application.
export const fetchResults = async (params: FetchResultsParams): Promise<Result[] | null> => {
  if (!params.id) {
    throw new Error("Competition ID is required");
  }

  try {
    // Fetching the results from the API endpoint
    const response = await axiosInstance.get('result/all-results', {
      params: {
        PageNumber: params.pageNumber,
        PageSize: params.pageSize,
        OrderBy: params.orderBy,
      },
    });

    const data = response.data;

    // Check if data is structured properly and return it.
    const formattedResults = data?.$values?.map((competition: any) => {
      return competition.distances.$values.map((distance: any) => {
        return {
          competitionId: competition.competitionId,
          distanceName: distance.distanceName,
          gunTime: distance.gunTime,  // gunTime extracted here
          ageCategory: distance.ageCategory,
          participants: distance.participants.$values.map((participant: any) => ({
            personId: participant.$id,
            firstName: participant.firstName,
            lastName: participant.lastName,
            city: participant.city,
            runTime: participant.runTime,  // runTime extracted here
            clublogo: participant.clublogo, // Add the clublogo here
          })),
        };
      });
    }).flat(); // Flatten the array of results for easy use

    return formattedResults;

  } catch (error: any) {
    console.error(
      'Failed to fetch results:',
      error?.response?.data || error.message
    );
    return null;
  }
};

/**
 * Upload a CSV file for a specific result
 * @param id - The result ID to which the CSV is related
 * @param file - The file to be uploaded
 * @returns true if the upload is successful
 */
export const uploadCsv = async (id: string, file: File): Promise<boolean> => {
  try {
    const formData = new FormData();
    formData.append('FormFile', file);

    const response = await axiosInstance.post(`Result/${id}`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });

    return response.status === 201;
  } catch (error: any) {
    if (error.response) {
      console.error('Failed to upload CSV file:', error.response.data);
    } else {
      console.error('Failed to upload CSV file:', error.message);
    }

    throw new Error(error.response?.data || 'Failed to upload CSV');
  }
};

/**
 * Update the run time for a specific result
 * @param registrationId - The ID of the registration for the result
 * @param runTime - The new run time to be updated
 * @returns true if the update was successful
 */
export const updateResultRunTime = async (
  registrationId: string,
  runTime: { hours: number; minutes: number; seconds: number }
): Promise<boolean> => {
  try {
    // Format the run time to HH:MM:SS
    const formattedRunTime = `${String(runTime.hours).padStart(
      2,
      '0'
    )}:${String(runTime.minutes).padStart(2, '0')}:${String(
      runTime.seconds
    ).padStart(2, '0')}`;

    const response = await axiosInstance.put(`Result/runtime/${registrationId}`, {
      runTime: formattedRunTime,
    });

    return response.status === 201;
  } catch (error: any) {
    console.error(
      'Failed to update result runtime:',
      error?.response?.data || error.message
    );
    return false;
  }
};

/**
 * Update the gun time for a specific competition
 * @param competitionId - The ID of the competition
 * @param gunTime - The new gun time to be updated
 * @returns true if the update was successful
 */
export const updateResultGunTime = async (
  competitionId: number,
  gunTime: Date
): Promise<boolean> => {
  try {
    // Format the gun time as ISO string
    const formattedGunTimeString = gunTime.toISOString();

    const response = await axiosInstance.put(`Result/guntime/${competitionId}`, {
      gunTime: formattedGunTimeString,
    });

    return response.status === 201;
  } catch (error: any) {
    console.error(
      'Failed to update result gun time:',
      error?.response?.data || error.message
    );
    return false;
  }
};
