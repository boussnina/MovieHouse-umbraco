async function handleSearch() {
    const query = document.getElementById('searchInput').value;

    if (!query) {
        alert("Please enter a movie name.");
        return;
    }

    try {
        const response = await fetch(`/api/moviesearch/search?query=${encodeURIComponent(query)}`);
        if (!response.ok) {
            throw new Error("Failed to fetch movie data.");
        }

        const data = await response.json();
        console.log("Movie results:", data);

        return data;

    } catch (error) {
        console.error("Error:", error);
        alert("Something went wrong while searching for the movie.");
    }
}

function displayResults(movies) {
    const container = document.getElementById("searchResults");
    container.innerHTML = "";

    movies.forEach(movie => {
        const movieElement = document.createElement("div");
        movieElement.classList.add("movie-result");

        movieElement.innerHTML = `
            <h3>${movie.title} (${movie.release_date?.substring(0, 4) || "N/A"})</h3>
            <p>${movie.overview || "No description available."}</p>
            ${movie.poster_path 
                ? `<img src="https://image.tmdb.org/t/p/w200${movie.poster_path}" alt="${movie.title} poster" />`
                : ""
            }
            <hr />
        `;

        container.appendChild(movieElement);
    });
}
